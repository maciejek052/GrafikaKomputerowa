using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Zadanie6
{
    internal class Bezier
	{
		private const float dt = 0.01f;
		private static long BinomialCoefficient(long n, long k)
		{
			if ((n == k) || (k == 0))
				return 1;
			else
				return BinomialCoefficient(n - 1, k) + BinomialCoefficient(n - 1, k - 1);
		}
		private static double GetX(double t, List<Point> points)
		{
			double sum = 0.0;
			int n = points.Count - 1;
			foreach (var item in points)
			{
				int i = points.IndexOf(item);
				sum += item.X * BinomialCoefficient(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
			}
			return sum;
		}
		private static double GetY(double t, List<Point> points)
		{
			double sum = 0.0;
			int n = points.Count - 1;
			foreach (var item in points)
			{
				int i = points.IndexOf(item);
				sum += item.Y * BinomialCoefficient(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
			}
			return sum;
		}
		public static List<Point> GetPoints(List<Point> controlPoints)
		{
			List<Point> points = new List<Point>();
			for (float t = 0.0f; t < 1.0; t += dt)
			{
				points.Add(new Point(GetX(t, controlPoints), GetY(t, controlPoints)));
			}
			points.Add(new Point(GetX(1.0f, controlPoints), GetY(1.0f, controlPoints)));
			return points;
		}
	}
}
