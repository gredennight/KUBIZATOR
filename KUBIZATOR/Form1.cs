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
using System.Windows;
using System.Numerics;

namespace KUBIZATOR
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
            Console output = new Console(richTextBox1);
            string path = "C:\\Users\\grede\\Downloads\\untitled.obj";
            MeshContainer meshContainer = new MeshContainer();
            meshContainer.LoadModel(path);
            /*
            output.WriteLog("путин хуйлуша");
            output.WriteLog("путин хуйлуша", Color.Yellow);
            output.WriteLog("путин хуйлуша", Color.Blue);
            */
            output.WriteLog(Convert.ToString(meshContainer.PolysMaxDimension()));
            /*
            Point3D test = new Point3D(1, 5, 1);
            test.Normalize();
            output.WriteLog(Convert.ToString(test.X)+" "+ Convert.ToString(test.Y)+" "+ Convert.ToString(test.Z));
            */
            // output.ClearLog();

            /*
            Point3D point = new Point3D();
            meshContainer.GetIntersection(new Point3D(-2, -1, 0), new Point3D(1, -1, 0), new Point3D(1, 1, 2), new Point3D(0, -1, 1-4), new Point3D(0, 1, 1-4), ref point);
            output.WriteLog(point.ToString());
            */
            //meshContainer.GetSize();
            meshContainer.GridSize = 256;
            //output.WriteLog( meshContainer.Voxelize());
            /*
            DrawImage.DrawArray(Color.Green,0);
            DrawImage.DrawArray(Color.Red,1);
            DrawImage.DrawArray(Color.Blue,2);
            */
            meshContainer.VoxelizeFractal();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
    
    



}
