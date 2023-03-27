// See https://aka.ms/new-console-template for more information

using System.Numerics;
using DotNetAidLib.Core.Drawing;

Console.WriteLine("Hello, World!");
BitMap<byte> p=new BitMap<byte>(5,5);
p.Points[4,4]=new DotNetAidLib.Core.Collections.Vector<byte>(2,5,6);
p.Points[4,4][0]=9;