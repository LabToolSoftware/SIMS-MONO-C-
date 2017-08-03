using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SIMS
{
    class FileReader
    {

        private double[,] _spectrumData;
        public FileReader()
        {

        }

        public double[,] ReadSpectrum()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {

                var file = File.ReadAllLines(ofd.FileName);

                _spectrumData = new double[file.Length, 2];

                int linenum = 0;
                for (int i = 0; i < file.Length; i++)
                {
                    if (file[i] != null)
                    {
                        string[] split = file[i].Split(',');

                        _spectrumData[i, 0] = double.Parse(split[0]);
                        _spectrumData[i, 1] = double.Parse(split[1]);
                    }
                }
            }
            return _spectrumData;

        }
    }



}
