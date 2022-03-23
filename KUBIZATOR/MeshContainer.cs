using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Windows;
using System.Numerics;
using System.Drawing.Imaging;

namespace KUBIZATOR
{
    /// <summary>
    /// класс для работы с OBJ файлами
    /// </summary>
    public class MeshContainer
    {
        //-----------------------------------бесполезные переменные
        /// <summary>
        /// путь к файлу obj
        /// </summary>
        private string PathToObject;

        /// <summary>
        /// максимальный размер меша
        /// </summary>
        private double Size;

        /// <summary>
        /// размер меша в разных осях
        /// </summary>
        private Point3D AxisSize=new Point3D();

        /// <summary>
        /// что-то вроде начала координат для трассировки
        /// </summary>
        private Point3D StartPoint;

        //-----------------------------------полезные переменные

        /// <summary>
        /// массив точек
        /// </summary>
        public List<Point3D> Verts = new List<Point3D>();
        /// <summary>
        /// массив полигонов, состояших из n вершин
        /// </summary>
        public List<List<int>> Polys = new List<List<int>>();
        /// <summary>
        /// массив блоков в координатной сетке и с айдишниками
        /// </summary>
        public List<BlockInfo> Blocks = new List<BlockInfo>();
        /// <summary>
        /// размер сетки
        /// </summary>
        public int GridSize { get; set; }

        public List<List<List<int>>> BlocksMas = new List<List<List<int>>>();



        //-----------------------------------методы
        
        /// <summary>
        /// получить размер объекта 
        /// </summary>
        /// <returns></returns>
        public void GetSize()
        {
            double minx = Verts[0].X, miny = Verts[0].Y, minz = Verts[0].Z;
            double maxx = Verts[0].X, maxy = Verts[0].Y, maxz = Verts[0].Z;
            //double xsize,mxsize,ysize,mysize, zsize, totalSize;

            //получаем начало трассировка, т.е. точку с минимальными координатами
            for (int i = 0; i < Verts.Count; i++)
            {
                if (Verts[i].X < minx) { minx = Verts[i].X;}
                else if(Verts[i].X > maxx) { maxx = Verts[i].X;}
                if (Verts[i].Y < miny) { miny = Verts[i].Y;}
                else if(Verts[i].Y > maxy) { maxy = Verts[i].Y;}
                if (Verts[i].Z < minz) { minz = Verts[i].Z; }
                else if (Verts[i].Z > maxz) { maxz = Verts[i].Z;}
            }
            this.StartPoint =new Point3D(minx, miny, minz);

            //получаем размеры модели по осям
            //Point3D AxisSize=new Point3D();
            this.AxisSize.X = Math.Abs(minx) + maxx;
            this.AxisSize.Y = Math.Abs(miny) + maxy;
            this.AxisSize.Z = Math.Abs(minz) + maxz;

            this.Size=Math.Max(Math.Max(AxisSize.X,AxisSize.Y),AxisSize.Z);



        }
        /// <summary>
        /// Вокселизировать модель
        /// </summary>
        /// <param name="GridSize">размер сетки</param>
        public string Voxelize()
        {
            Blocks.Clear();
            //получаем шаг луча для каждой итерации проверки пересечения с мешем
            double step = Size / GridSize;
            string outputString="";
            //всякие точки
            Point3D P1 = new Point3D();
            Point3D P2 = new Point3D();
            Point3D Intersection = new Point3D();
            //дополнительный массив полигонов, на случай если я добавлю сортировку
            for (int z = 0; z < GridSize; z++)
            {
                //вот сюда надо вставить сортировку полигонов по высоте, чтобы для каждого ертикального слоя были только те полигоны, которые в нём учавствуют
                List<List<int>> matrix = new List<List<int>>();
                for(int y=0; y < GridSize; y++)
                {
                    List<int> list = new List<int>();

                    for (int x=0; x < GridSize; x++)
                    {
                        //начальная точка отрезка для поиска пересечения
                        P1 = new Point3D(StartPoint.X+(step * x), StartPoint.Y+(step * y) + (step / 2), StartPoint.Z + (step * z) + (step / 2));
                        //конечная
                        P2 = new Point3D(StartPoint.X + (step * (x+1)), StartPoint.Y + (step * y) + (step / 2), StartPoint.Z + (step * z) + (step / 2));
                        bool consist=false;
                        //в цикле перебираем все полигоны
                        for(int i = 0; i < Polys.Count; i++)
                        {
                            if(this.GetIntersection(Verts[Polys[i][0] -1], Verts[Polys[i][1] - 1], Verts[Polys[i][2] - 1],P1,P2,ref Intersection))
                            {
                                Blocks.Add(new BlockInfo(x,y,z));
                                outputString += x + " " + y + " " + z + "\n";
                                consist=true;
                            }
                            
                        }
                        if (consist) { list.Add(1); }
                        else { list.Add(0); }

                    }
                    matrix.Add(list);
                }
                DrawImage.DrawArray(matrix, Color.Black, z);
            }
            return outputString;
            
        }

        /// <summary>
        /// вокселизация объекта на основе разбиения полигонов
        /// </summary>
        /// <returns></returns>
        public bool VoxelizeFractal()
        {
            //проверяем максимальное число вершин в полигонах, метод нормально работает только для треугольников
            if (PolysMaxDimension() != 3)
            {
                return false;
            }
            
            //очищаем всякую фигню
            Blocks.Clear();
            BlocksMas.Clear();
            

            //разбиваем объект на микрочеликов на полигонах
            double step = Size / GridSize;//длина стороны куба сетки
            int count = 0;
            while (count < Polys.Count)//пока не пройдём каждый элемент в массиве полигонов
            {
                while (MaxLenght(Polys[count]) >= step)//пока длины сторон полигонов больше стороны куба разрезаем их
                {
                    SubdivideTriangle(count);//смешная нарезка полигонов
                }

                count++;
            }

            //инициалиебатьеговрот создаём массив в форме куба и заполняем его 0
            //формат массива BlocksMas[z][y][x]
            this.BlocksMas = new List<List<List<int>>>();
            for(int z = 0; z <= GridSize; z++)
            {
                List<List<int>> xylist = new List<List<int>>();
                for(int y = 0; y <= GridSize; y++)
                {
                    List<int> xlist = new List<int>();
                    for(int x = 0; x <= GridSize; x++)
                    {
                        xlist.Add(0);
                    }
                    xylist.Add(xlist);
                }
                this.BlocksMas.Add(xylist);
            }

            //заполняем массив блоков
            foreach(var vert in Verts)
            {
                int x = Convert.ToInt32((vert.X - StartPoint.X) / step);
                int y = Convert.ToInt32((vert.Y - StartPoint.Y) / step);
                int z = Convert.ToInt32((vert.Z - StartPoint.Z) / step);
                this.BlocksMas[z][y][x] = 1;
            }

            //рисуем его построчно
            for(int i = 0; i < this.BlocksMas.Count; i++)
            {
                DrawImage.DrawArray(this.BlocksMas[i], Color.Black, i);
            }
            return true;
        }

        /// <summary>
        /// вернуть максимальную длину стороны треугольника
        /// </summary>
        /// <param name="Polys">строка из массива полигонов</param>
        /// <returns></returns>
        public double MaxLenght(List<int> Polys)
        {
            double ab = Point3D.getLength(Verts[Polys[0] - 1], Verts[Polys[1] - 1]);
            double bc = Point3D.getLength(Verts[Polys[1] - 1], Verts[Polys[2] - 1]);
            double ca = Point3D.getLength(Verts[Polys[2] - 1], Verts[Polys[0] - 1]);


            return Math.Max(Math.Max(ab,bc),ca);
        }

        /// <summary>
        /// разделить один полигон на 4 мелких
        /// </summary>
        /// <param name="index">номер полигона в массиве</param>
        public void SubdivideTriangle(int index)
        {
            //получаем точки исходного треугольника
            Point3D A = this.Verts[Polys[index][0] - 1];
            Point3D B = this.Verts[Polys[index][1] - 1];
            Point3D C = this.Verts[Polys[index][2] - 1];
            
            //сами значения абс на всякий случай
            int a= Polys[index][0],b= Polys[index][1],c= Polys[index][2];
            //получаем середины его сторон
            Point3D A1 = Point3D.getMedianPoint(A, B);
            Point3D B1 = Point3D.getMedianPoint(B, C);
            Point3D C1 = Point3D.getMedianPoint(C, A);

            //поиск точек пересечения среди уже имеющихся
            /*
            #region
            int a1,b1,c1;
            a1 = Verts.IndexOf(A1);
            b1 = Verts.IndexOf(B1);
            c1 = Verts.IndexOf(C1);

            if (a1 == -1)
            {
                this.Verts.Add(A1);
                a1 = Verts.Count;
            }
            if (b1 == -1)
            {
                this.Verts.Add(B1);
                b1 = Verts.Count;
            }
            if (c1 == -1)
            {
                this.Verts.Add(C1);
                c1 = Verts.Count;
            }
            #endregion

            //перезаписываем точки B и C в исходном полигоне
            this.Polys[index][1] = a1;
            this.Polys[index][2] = c1;

            //дописываем оставшиеся три полигона
            this.Polys.Add(new List<int> { a1, b1, c1 });
            this.Polys.Add(new List<int> { a1, b, b1 });
            this.Polys.Add(new List<int> { c1, b1, c });
            */
            #region старый варик
            
            // метод с добавлением огромного числа вершин
            //добавляем новые точки в массив
            this.Verts.Add(A1);//count-2
            this.Verts.Add(B1);//count-1
            this.Verts.Add(C1);//count

            //перезаписываем точки B и C в исходном полигоне
            this.Polys[index][1] = Verts.Count - 2;
            this.Polys[index][2] = Verts.Count;

            //дописываем оставшиеся три полигона
            this.Polys.Add(new List<int> { Verts.Count - 2, Verts.Count - 1, Verts.Count });
            this.Polys.Add(new List<int> { Verts.Count - 2,b, Verts.Count - 1});
            this.Polys.Add(new List<int> {Verts.Count,Verts.Count-1,c});
            
            #endregion


        }

        /// <summary>
        /// получить площадь треугольника по герону
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        private double getGeronArea(Point3D A,Point3D B, Point3D C)
        {
            double AB, BC, CA, p;
            AB= Point3D.getLength(A,B);
            BC= Point3D.getLength(B,C);
            CA= Point3D.getLength(C,A);
            p = (AB + BC + CA) / 2;
            return Math.Sqrt(p*(p-AB)*(p-BC)*(p-CA));
        }

        /// <summary>
        /// Проверить есть ли точка пересечения отрезка и треугольника, если да, то вывести эту точку
        /// </summary>
        /// <param name="A">Точка А треугольника</param>
        /// <param name="B">Точка Б треугольника</param>
        /// <param name="C">Точка С треугольника</param>
        /// <param name="P1">Точка начала отрезка</param>
        /// <param name="P2">Точка конца отрезка</param>
        /// <param name="Intersection">Точка пересечения</param>
        /// <returns></returns>
        public bool GetIntersection(Point3D A, Point3D B, Point3D C, Point3D P1, Point3D P2, ref Point3D Intersection)
        {
            double d;
            //double a1, a2, a3, total;
            double denom, mu;
            Point3D n = new Point3D();
            Point3D pa1 = new Point3D();
            Point3D pa2 = new Point3D();
            Point3D pa3 = new Point3D();

            //параметры плоскости из полигона
            n.X = (B.Y - A.Y) * (C.Z - A.Z) - (B.Z - A.Z) * (C.Y - A.Y);
            n.Y = (B.Z - A.Z) * (C.X - A.X) - (B.X - A.X) * (C.Z - A.Z);
            n.Z = (B.X - A.X) * (C.Y - A.Y) - (B.Y - A.Y) * (C.X - A.X);
            n.Normalize();
            d = -n.X * A.X - n.Y * A.Y - n.Z * A.Z;

            //получить место пересечения с плоскостью
            denom = n.X * (P2.X - P1.X) + n.Y * (P2.Y - P1.Y) + n.Z * (P2.Z - P1.Z);
            if (Math.Abs(denom) < double.Epsilon)
            {
                return false;
            }
            mu = -(d + n.X * P1.X + n.Y * P1.Y + n.Z * P1.Z) / denom;
            Intersection.X = P1.X + mu * (P2.X - P1.X);
            Intersection.Y = P1.Y + mu * (P2.Y - P1.Y);
            Intersection.Z = P1.Z + mu * (P2.Z - P1.Z);
            if (mu < 0 || mu > 1)
            {
                return false;
            }
            //проверка принадлежности точки треугольнику по сопос... блядь, короче проверка того будет ли треугольников, на которые делит точка полигон одинаковая площадь
            double temp1 = this.getGeronArea(A, B, Intersection);
            double temp2 = this.getGeronArea(B, C, Intersection);
            double temp3 = this.getGeronArea(C, A, Intersection);
            double SmallOnes = temp1 + temp2 + temp3;
            double BigOne = this.getGeronArea(A,B,C);

            if (Math.Round(SmallOnes,6)==Math.Round(BigOne,6))
            {
                return true;
            }
            /*
             * походу нерабочее
            //проверка принадлежности точки треугольнику через операции над векторами
            Point3D temp = new Point3D(0, 0, 0);
            Point3D AB = temp.getVector(A, B);
            Point3D BC = temp.getVector(B, C);
            Point3D CA = temp.getVector(C, A);
            Point3D AP = temp.getVector(A, Intersection);
            Point3D BP = temp.getVector(B, Intersection);
            Point3D CP = temp.getVector(C, Intersection);
            Point3D Vec1 = temp.vectorMultiply(AB, AP);
            Point3D Vec2 = temp.vectorMultiply(BC, BP);
            Point3D Vec3 = temp.vectorMultiply(CA, CP);
            */





            /*
            //проверить принадлежит ли точка пересечения треугольку
            pa1.X = A.X - Intersection.X;
            pa1.Y = A.Y - Intersection.Y;
            pa1.Z = A.Z - Intersection.Z;
            pa1.Normalize();

            pa2.X = B.X - Intersection.X;
            pa2.Y = B.Y - Intersection.Y;
            pa2.Z = B.Z - Intersection.Z;
            pa2.Normalize();

            pa3.X = C.X - Intersection.X;
            pa3.Y = C.Y - Intersection.Y;
            pa3.Z = C.Z - Intersection.Z;
            pa3.Normalize();

            a1 = pa1.X * pa2.X + pa1.Y * pa2.Y + pa1.Z * pa2.Z;
            a2 = pa2.X * pa3.X + pa2.Y * pa3.Y + pa2.Z * pa3.Z;
            a3 = pa3.X * pa1.X + pa3.Y * pa1.Y + pa3.Z * pa1.Z;
            total =(180/Math.PI)*(Math.Acos(a1) + Math.Acos(a2) + Math.Acos(a3));
            */
            return false;
        }
        /// <summary>
        /// преобразовать все полигоны модели в треугольники
        /// </summary>
        public void Triangulate()
        {
            switch (PolysMaxDimension())
            {
                case 4:
                    for (int i = 0; i < Polys.Count; i++)
                    {
                        if (Polys[i].Count > 3)
                        {
                            List<int> list = new List<int>(Polys[i]);
                            list.RemoveAt(0);
                            Polys[i].RemoveAt(Polys[i].Count - 1);
                            Polys.Add(list);
                        }
                    }
                    break;
                case 3:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// максимальное число вершин в полигонах
        /// </summary>      
        public int PolysMaxDimension()
        {
            //этот варик считывает данные из файла, а надо из массива полигонов
            /*
            int ReturnValue = 0;
            string[] lines = File.ReadAllLines(this.PathToObject);
            foreach (string line in lines)
            {
                if (line.ToLower().StartsWith("f"))
                {
                    var vx = line.Split(' ').Skip(1).Select(v => Int32.Parse(v)).ToArray();
                    if (vx.Length > ReturnValue)
                    {
                        ReturnValue = vx.Length;
                    }
                }

            }*/
            int ReturnValue = Polys[0].Count;
            foreach(var poly in Polys)
            {
                if (poly.Count > ReturnValue)
                {
                    ReturnValue = poly.Count;
                }
            }

            return ReturnValue;
        }
        /// <summary>
        /// открывает указанный obj файл и загружает его в массивы
        /// </summary>
        /// <param name="Path">Путь к obj файлу</param>
        public void LoadModel(string Path)
        {
            PathToObject = Path;
            Verts.Clear(); Polys.Clear();
            string[] Lines = File.ReadAllLines(Path);
            
            //считывание файла в массивы точек и полигонов
            foreach (string Line in Lines)
            {
                //если строка начинается с v, то записываем новую вершину в массив
                if (Line.ToLower().StartsWith("v"))
                {
                    var vx = Line.Split(' ').Skip(1).Select(v => Double.Parse(v.Replace(".", ","))).ToArray();
                    Verts.Add(new Point3D(vx[0], vx[1], vx[2]));
                }
                //если строка описывает полигон
                else if (Line.ToLower().StartsWith("f"))
                {

                    var vx = Line.Split(' ').Skip(1).Select(v => Int32.Parse(v)).ToArray();
                    //polygons.Add(new Tuple<int, int, int>(vx[0], vx[1], vx[2]));
                    List<int> PolysTemp = new List<int>();
                    for (int i = 0; i < vx.Length; i++)
                    {
                        PolysTemp.Add(vx[i]);
                    }
                    Polys.Add(PolysTemp);
                }


            }

            //попытка триангулировать полигоны
            switch (PolysMaxDimension())
            {
                case 3://максимальное число вершин в полигоне 3

                    break;
                case 4://максимальное число вершин в полигоне 4

                    Triangulate();
                    //1 триангулировать модель
                    //2 то же самое, что и в 3м кейсе
                    break;
                default://всё остальное

                    break;

            }

            //получение размера модели
            GetSize();

        }
    }
    /// <summary>
    /// Контейнер точки в 3д пространстве
    /// </summary>
    public class Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// возвращает точку по центру отрезка
        /// </summary>
        /// <param name="A">начало отрезка</param>
        /// <param name="B">конец отрезка</param>
        /// <returns></returns>
        public static Point3D getMedianPoint(Point3D A,Point3D B)
        {
            return new Point3D((A.X + B.X) / 2, (A.Y + B.Y) / 2, (A.Z + B.Z) / 2);
        }
        /// <summary>
        /// возвращает расстояние между точками
        /// </summary>
        /// <returns></returns>
        public static double getLength(Point3D A,Point3D B)
        {

            return Math.Sqrt(Math.Pow(B.X - A.X, 2) + Math.Pow(B.Y - A.Y, 2) + Math.Pow(B.Z - A.Z, 2));
        }
        /// <summary>
        /// нормализация точки, если считать её вектором
        /// </summary>
        /// <returns></returns>
        public Point3D Normalize()
        {
            double maxx = Math.Abs(this.X), maxy = Math.Abs(this.Y), maxz = Math.Abs(this.Z), length = Math.Max(maxx, Math.Max(maxy, maxz));
            this.X /= length;
            this.Y /= length;
            this.Z /= length;
            /*
             * код написанный пидорасами, которые сами его не проверяли, пусть идут нахуй
            double length=1.0f/Math.Sqrt(this.X*this.X+this.Y*this.Y+this.Z*this.Z);
            this.X*=length;
            this.Y*=length;
            this.Z*=length;
            */
            return this;
        }
        /// <summary>
        /// получить вектор из двух точек
        /// </summary>
        /// <param name="A">начальная точка</param>
        /// <param name="B">конечная точка</param>
        /// <returns></returns>
        public Point3D getVector(Point3D A, Point3D B)
        {
            return new Point3D(B.X-A.X, B.Y-A.Y, B.Z-A.Z);
        }
        /// <summary>
        /// получить векторное произведение двух векторов
        /// </summary>
        /// <param name="A">первый вектор</param>
        /// <param name="B">второй вектор</param>
        /// <returns></returns>
        public Point3D vectorMultiply(Point3D A,Point3D B)
        {
            return new Point3D(A.Y * B.Z - A.Z * B.Y, A.Z * B.X - A.X * B.Z, A.X * B.Y - A.Y * B.X);
         }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Point3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        override public string ToString()
        {
            string temp = Convert.ToString(this.X + " " + this.Y + " " + this.Z);
            return temp;
        }
    }
    /// <summary>
    /// контейнер блоков по координатам XYZ и id блока
    /// </summary>
    public class BlockInfo
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public double ID { get; set; }
        public BlockInfo(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            ID = 0;
        }
        public BlockInfo(int x, int y, int z, int id)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.ID = id;
        }
    }

    /// <summary>
    /// удобный вывод в консоль
    /// </summary>
    public class Console
    {
        /// <summary>
        /// установить вывод
        /// </summary>
        /// <param name="output">ссылка на объект</param>
        public Console(RichTextBox output)
        {
            this.output = output;
        }
        private RichTextBox output;
        #region //для работы с консолью
        /// <summary>
        /// вывести в консоль текст
        /// </summary>
        /// <param name="text">текст на вывод</param>
        public void WriteLog(string text)
        {
            this.output.SelectionStart = this.output.TextLength;
            this.output.SelectionLength = 0;
            this.output.SelectionColor = Color.Black;
            this.output.AppendText(text + "\n");
            this.output.SelectionColor = this.output.ForeColor;
        }
        /// <summary>
        /// вывести в консоль текст заданного цвета
        /// </summary>
        /// <param name="text">текст на вывод</param>
        /// <param name="color">цвет текста</param>
        public void WriteLog(string text, Color color)
        {
            this.output.SelectionStart = this.output.TextLength;
            this.output.SelectionLength = 0;
            this.output.SelectionColor = color;
            this.output.AppendText(text + "\n");
            this.output.SelectionColor = this.output.ForeColor;
        }
        public void ClearLog()
        {
            this.output.Clear();
        }

        #endregion

    }
    /// <summary>
    /// класс для малювання и всё такое
    /// </summary>
    public class DrawImage
    {
        /// <summary>
        /// короч, это рисует двумерную матрицу в формате пикчи
        /// </summary>
        /// <param name="matrix">сама матрица</param>
        /// <param name="color">цвет заполненных ячеек</param>
        /// <param name="count">номер файла</param>
        public static void DrawArray(List<List<int>> matrix, Color color, int count)
        {
            Bitmap bitmap = new Bitmap(Convert.ToInt32(1024), Convert.ToInt32(1024), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bitmap);
            //команды рисования
            g.Clear(Color.White);
            double step = 1024 / matrix.Count;
            for (int h = 0; h < matrix.Count; h++)
            {
                for (int w = 0; w < matrix[h].Count; w++)
                {
                    if (matrix[h][w] != 0)
                    {
                        g.FillRectangle(Brushes.Black, Convert.ToInt32(step * w), Convert.ToInt32(1024 - step * h), Convert.ToInt32(step), Convert.ToInt32(step));
                    }
                }
            }
            //g.DrawRectangle(new Pen(color),)
            string temp = ".\\" + Convert.ToString(count) + ".png";
            bitmap.Save(temp, ImageFormat.Png);

        }
        #region
        /*
        public static void DrawBlockLayers(List<BlockInfo> blocks,Color color)
        {
            Bitmap bitmap = new Bitmap(Convert.ToInt32(1024), Convert.ToInt32(1024), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bitmap);
            //команды рисования
            g.Clear(Color.White);
            double step = 1024 / matrix.Count;
            for (int h = 0; h < matrix.Count; h++)
            {
                for (int w = 0; w < matrix[h].Count; w++)
                {
                    if (matrix[h][w] != 0)
                    {
                        g.DrawRectangle(new Pen(color), Convert.ToInt32(step * w), Convert.ToInt32(1024 - step * h), Convert.ToInt32(step), Convert.ToInt32(step));
                    }
                }
            }
            //g.DrawRectangle(new Pen(color),)
            string temp = ".\\" + Convert.ToString(count) + ".png";
            bitmap.Save(temp, ImageFormat.Png);
        }
        */
        #endregion
        /// <summary>
        /// Отрисовать массив кубов на картинке
        /// </summary>
        /// <param name="Blocks">Массив кубов формата [z][y][x]</param>
        /// <param name="top">цвет верхней грани</param>
        /// <param name="left">цвет левой грани</param>
        /// <param name="right">цвет правой грани</param>
        public static void DrawVisual(List<List<List<int>>> Blocks,Color top, Color left, Color right)
        {
            //разрешение картинки, соотношение буду использовать 4:3
            int height = 1200, widht = 800;

            //начальные координаты сетки
            Point start = new Point(widht/2, height/2);

            //отступы по координатам
            int hstep = (height / 2) / 2 * Blocks.Count;
            int wstep = widht / 2 * Blocks.Count;
            int zstep = 2 * hstep;
            
        }

        /// <summary>
        /// преобразование 3д координат в изометрию
        /// </summary>
        /// <param name="x">хз что это</param>
        /// <param name="y">аналогично</param>
        /// <param name="z">понятия не имею</param>
        /// <param name="start">точка начала отсчёта</param>
        /// <param name="hstep">шаг высоты</param>
        /// <param name="wstep">шаг широты</param>
        /// <param name="zstep">дополнительный шаг высоты</param>
        /// <returns></returns>
        private static Point getIzoCoords(int x,int y,int z,Point start,int hstep,int wstep,int zstep)
        {
            Point IzoCoords = new Point(
                start.X+(x-y)*wstep,
                start.Y+(x+y)*hstep-z*zstep
                );
            return IzoCoords;
        }
    }
}
