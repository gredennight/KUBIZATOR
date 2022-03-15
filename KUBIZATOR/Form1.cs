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
            /// массив полигонов, состояших из 3 вершин
            /// </summary>
            public List<Point3D> Polys = new List<Point3D>();
            public int PolysDimension(string path)
            {
                return 0;
            }

            /// <summary>
            /// открывает указанный obj файл и загружает его в массивы
            /// </summary>
            /// <param name="Path">Путь к obj файлу</param>
            public void LoadModel(string Path)
            {
                switch
                Verts.Clear(); Polys.Clear();
                string[] Lines = File.ReadAllLines(Path);
                foreach (string Line in Lines)
                {
                    //если строка начинается с v, то записываем новую вершину в массив
                    if (Line.ToLower().StartsWith("v"))
                    {
                        var vx =Line.Split(' ').Skip(1).Select(v=>Double.Parse(v.Replace(".",","))).ToArray();
                        Verts.Add(new Point3D(vx[0],vx[1],vx[2]));
                    }
                    //если строка описывает полигон
                    if (Line.ToLower().StartsWith("f"))
                    {
                     
                        var vx = Line.Split(' ').Skip(1).Select(v => Double.Parse(v.Replace(".", ","))).ToArray();

                    }


                }
            }
        }
    }
}
