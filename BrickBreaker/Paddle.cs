using System;
using System.Collections.Generic;
using System.Text;

namespace BrickBreaker
{
    public class Paddle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }



        public Paddle(int x, int y, int width, int height)
        {
            X = x; Y = y; Width = width; Height = height;
        }
        public void Extend(int amount)
        {
            Width += amount;
        }
        public void SetWidth(int newWidth)
        {
            Width = newWidth;
        }

    }
}
