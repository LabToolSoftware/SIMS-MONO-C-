using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;


namespace SIMS
{
    class SIMSDecomposer
    {
        private double _stepsize;


        public Matrix<double> Decompose(int k, Matrix<double> raw_data)
        {

            double[] range_start = new double[k];
            double[] v_ms = new double[k];
            Matrix<double> converted_data = ConvertLambdas(raw_data);
            var result = Matrix<double>.Build.Dense(raw_data.RowCount, k + 1);

            double lowestscore = 0;
            double currentscore = 0;

            _stepsize = 5;

            List<int[]> combos = GetCombos(20, k);

            for (int i = 0; i < combos[0].Length; i++)
            {
                v_ms[i] = 10000000 / (300 + (combos[0][i] * _stepsize));
            }

            var Intens = GetIntensities(converted_data.Column(0), converted_data.Column(1), v_ms);
            lowestscore = CalcSScore(converted_data.Column(0), converted_data.Column(1), v_ms, Intens);

            foreach (var item in combos)
            {
                for (int i = 0; i < item.Length; i++)
                {
                    v_ms[i] = 10000000 / (300 + (item[i] * _stepsize));
                }

                Intens = GetIntensities(converted_data.Column(0), converted_data.Column(1), v_ms);
                currentscore = CalcSScore(converted_data.Column(0), converted_data.Column(1), v_ms, Intens);
                //MessageBox.Show(v_ms[0].ToString() + "," + v_ms[1].ToString() + " : " + Intens[0].ToString() + "," + Intens[1].ToString() + " : " + currentscore);
                if (currentscore < lowestscore)
                {
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < raw_data.RowCount; j++)
                        {
                            result[j, i] = CalcLogNormVals(converted_data.Column(0), v_ms)[j, i] * Intens[i];
                            result[j, k] += result[j, i];
                        }

                    }
                }
            }

            return result;
        }
        static List<int[]> GetCombos(int n, int k)
        {
            List<int[]> combos = new List<int[]>();
            int[] combo = new int[k + 2];
            int j = k;

            for (int i = 0; i < k; i++)
            {
                combo[i] = i;
            }

            combo[k] = n;

            while (true)
            {
                int[] item = new int[k];
                for (int i = 0; i < k; i++)
                {
                    item[i] = combo[i];
                }
                combos.Add(item);
                j = 0;
                while (combo[j] + 1 == combo[j + 1])
                {
                    combo[j] = j;
                    j++;
                }
                if (j > k - 1)
                {
                    return combos;
                }
                combo[j] += 1;
            }
        }
        private Matrix<double> ConvertLambdas(Matrix<double> rawdata)
        {
            int length = rawdata.RowCount;
            int k = rawdata.ColumnCount;
            var converteddata = Matrix<double>.Build.Dense(length, k);
            for (int i = 0; i < k; i++)
            {
                for (int j = 1; j < length; j++)
                {
                    converteddata[j, 0] = 10000000 / rawdata[j, 0];
                    converteddata[j, i] = rawdata[j, i] * (rawdata[j, 0] / 10000000) * (rawdata[j, 0] / 10000000);

                }
            }
            return converteddata;
        }
        private double CalcSScore(Vector<double> raw_wavenumbers, Vector<double> raw_intensities, double[] v_ms, Vector<double> I_ms)
        {
            int num_vm = v_ms.Length;
            int num_wavelengths = raw_wavenumbers.Count;

            Matrix<double> componentlogvals = CalcLogNormVals(raw_wavenumbers, v_ms);

            double score = 0;

            for (int i = 0; i < num_wavelengths; i++)
            {
                double sum = 0;
                for (int j = 0; j < num_vm; j++)
                {
                    sum += componentlogvals[i, j] * I_ms[j];
                }

                score += (sum - raw_intensities[i]) * (sum - raw_intensities[i]);
            }
            //MessageBox.Show(score.ToString());
            return score;
        }
        private Vector<double> GetIntensities(Vector<double> wavenumbers, Vector<double> raw_intensities, double[] v_ms)
        {

            Matrix<double> componentlogvals = CalcLogNormVals(wavenumbers, v_ms);
            double[,] coeff_matrix = new double[v_ms.Length, v_ms.Length];
            double[] result_vector = new double[v_ms.Length];

            //for (int i = 0; i < componentlogvals.ColumnCount; i++)
            //{
            //    for (int j = 0; j < componentlogvals.ColumnCount; j++)
            //    {
            //        double sum = 0;
            //        for (int k = 0; k < componentlogvals.RowCount; k++)
            //        {
            //            sum += componentlogvals[k, i] * componentlogvals[k, j];
            //        }
            //        coeff_matrix[i, j] = sum;
            //    }
            //}

            //for (int i = 0; i < coeff_matrix.GetLength(0); i++)
            //{
            //    double result_sum = 0;
            //    for (int j = 0; j < raw_intensities.Count; j++)
            //    {
            //        result_sum += raw_intensities[j] * componentlogvals[j, i];
            //    }
            //    result_vector[i] = result_sum;
            //}


            QR<double> qr = componentlogvals.QR();

            Matrix<double> Q1 = qr.Q.SubMatrix(0, componentlogvals.RowCount, 0, componentlogvals.ColumnCount);
            Matrix<double> R = qr.R.SubMatrix(0, componentlogvals.ColumnCount, 0, componentlogvals.ColumnCount);

            var intens = R.Inverse().Multiply(Q1.Transpose().Multiply(raw_intensities));

            //MessageBox.Show(R.ToString());

            return intens;
        }

        public Matrix<double> CalcLogNormVals(Vector<double> j_s, double[] v_m)
        {
            double[,] lognormval = new double[j_s.Count, v_m.Length];

            for (int i = 0; i < v_m.Length; i++)
            {
                double v_minus = (1.177 * v_m[i]) - 7780;
                double v_plus = (0.831 * v_m[i]) + 7070;

                double p = 0;
                p = (v_m[i] - v_minus) / (v_plus - v_m[i]);

                double a = 0;
                a = v_m[i] + ((p * (v_plus - v_minus)) / ((p * p) - 1));

                double lnterm = -1 * (Math.Log(2) / (Math.Log(p) * Math.Log(p)));

                for (int j = 0; j < j_s.Count; j++)
                {
                    double vterm = (Math.Log((a - j_s[j]) / (a - v_m[i]))) * (Math.Log((a - j_s[j]) / (a - v_m[i])));

                    lognormval[j, i] = Math.Exp(lnterm * vterm);
                    //MessageBox.Show(v_m[i] + " : " + " : " + j_s[j] + " : " + lognormval[j, i].ToString());
                }
            }

            var lognormmatrix = Matrix<double>.Build;
            var lognormval_matrix = lognormmatrix.DenseOfArray(lognormval);

            return lognormval_matrix;
        }

    }
}
