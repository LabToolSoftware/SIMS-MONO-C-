using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace SIMS
{
    public partial class Form1 : Form
    {
        private double[,] _spectrumdata;
        public Form1()
        {
            InitializeComponent();
        }

        private void openSpectrumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileReader fr = new FileReader();
            _spectrumdata = fr.ReadSpectrum();
            double[] wavelengths = new double[_spectrumdata.GetLength(0)];
            double[] intensities = new double[_spectrumdata.GetLength(0)];
            for (int i = 0; i < wavelengths.Length; i++)
            {
                wavelengths[i] = _spectrumdata[i, 0];
                intensities[i] = _spectrumdata[i, 1];
            }

            double max = intensities.Max();

            for (int i = 0; i < intensities.Length; i++)
            {
                intensities[i] = _spectrumdata[i, 1] / max;
            }
            chart1.Series["Original"].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            chart1.Series["Original"].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;

            for (int i = 0; i < _spectrumdata.GetLength(0); i++)
            {

                chart1.Series["Original"].Points.AddXY((10000000 / wavelengths[i]), (intensities[i] * (wavelengths[i] / 10000000) * (wavelengths[i] / 10000000)));
            }
        }

        private void decomposeSpectraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int k = int.Parse(textBox1.Text);
            SIMSDecomposer sd = new SIMSDecomposer();

            var a = Matrix<double>.Build;
            var spectrum = a.DenseOfArray(_spectrumdata);
            //MessageBox.Show(spectrum.Column(0) + " " + spectrum.Column(1));

            Matrix<double> vnI = sd.Decompose(k, spectrum);
            //MessageBox.Show(vnI.ToString());


            for (int i = 0; i < vnI.ColumnCount; i++)
            {
                chart1.Series.Add(i.ToString());
                chart1.Series[i.ToString()].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                for (int j = 0; j < vnI.RowCount; j++)
                {
                    //MessageBox.Show(vnI[j].ToString());
                    chart1.Series[i.ToString()].Points.AddXY(10000000 / spectrum[j, 0], vnI[j, i]);
                }
            }
        }
        //chart1.Series.Add("combined");
        //chart1.Series["combined"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        //for (int i = 0; i < wavelengths.Length; i++)
        //{
        //    double sum = 0;
        //    for (int j = 0; j < v_ms.GetLength(0); j++)
        //    {
        //        sum += (v_ms[j, 1] / ((v_ms[j, 0] / 10000000) * (v_ms[j, 0] / 10000000))) * sd.CalcLogNormVals(10000000 / wavelengths[i], 10000000 / v_ms[j, 0]);
        //    }
        //    chart1.Series["combined"].Points.AddXY((wavelengths[i]), sum);
        //}
    }
}

