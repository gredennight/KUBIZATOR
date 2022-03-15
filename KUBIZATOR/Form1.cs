using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace KUBIZATOR
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
            string path = "C:\\Users\\grede\\Downloads\\untitled.obj";
            MeshContainer meshContainer = new MeshContainer();
            meshContainer.LoadModel(path);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// контейнер точек в формате (x,y,z)
        /// </summary>
        public class Point3D
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Point3D(double x,double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        /// <summary>
        /// класс для работы с OBJ файлами
        /// </summary>
        public class MeshContainer
        {
            /// <summary>
            /// массив точек
            /// </summary>
            public List<Point3D> Verts = new List<Point3D>();
            /// <summary>
            /// массив полигонов, состояших из n вершин
            /// </summary>
            public List<List<int>> Polys = new List<List<int>>();
            /// <summary>
            /// максимальное число вершин в полигоне
            /// </summary>
            /// <param name="path">Путь к файлу</param>
            /// <returns></returns>
            public int PolysMaxDimension(string path)
            {
                int ReturnValue = 0;
                string []lines =File.ReadAllLines(path);
                foreach(string line in lines)
                {
                    if (line.ToLower().StartsWith("f"))
                    {
                        var vx = line.Split(' ').Skip(1).Select(v => Int32.Parse(v)).ToArray();
                        if(vx.Length > ReturnValue)
                        {
                            ReturnValue = vx.Length;
                        }
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
                Verts.Clear(); Polys.Clear();
                switch (PolysMaxDimension(Path))
                {
                    case 3://максимальное число вершин в полигоне 3
                        string[] Lines = File.ReadAllLines(Path);
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
                                Polys.Add(new List<int> { vx[0], vx[1], vx[2] });
                            }


                        }
                        break;
                    case 4://максимальное число вершин в полигоне 4
                        break;
                    default://всё остальное
                        break;

                }
                
            }
        }
    }
}
