using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Game_Of_Life_bitmap
{
    internal class cel
    {
        public int x { get; set; }
        public int y { get; set; }
        List<cel> buren { get; set; } = new List<cel>();
        public bool status { get; set; }
        public bool compare_status { get; set; }


        //FUNCTION TO GET ALL NEIGBOURS
        public void vulBuren(cel[,] grid, int Nx, int Ny)
        {
            /*
            for (int i = Math.Max(0, x-1); i < Math.Min(Nx, x+2); i++)
            {
                for (int j = Math.Max(0, y-1); j < Math.Min(Ny, y+2); j++)
                {
                    // Debug.WriteLine(Convert.ToString(x - 1 + i) + "   " + Convert.ToString(y - 1 + j));

                    if (i != x || j != y)
                    {
                        buren.Add(grid[i, j]);
                    }
                }
            }*/

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int xx = (i + x) % Nx;
                    int yy = (j + y) % Ny;

                    if (xx < 0)
                    {
                        xx += Nx;
                    }
                    if (yy < 0)
                    {
                        yy += Ny;
                    }

                    if (xx != x || yy != y)
                    {
                        buren.Add(grid[xx, yy]);
                    }
                }
            }
        }
        //FUNCTION TO CHECK HOW MANY NEIGHBOURS ARE ACTIVE
        public int BurenActief()
        {
            int result = 0;

            foreach (cel c in buren)
            {
                if (c.compare_status) result++;
            }

            return result;
        }

        //FUNCTION FOR UPDATING THE CELL STATUS
        public void UpdateCell()
        {
            int burenactief = BurenActief();

            if (compare_status)
            {
                if (burenactief == 2 || burenactief == 3)
                {
                    status = true;
                }
                else
                {
                    status = false;
                }
            }
            else
            {
                if (burenactief == 3)
                {
                    status = true;
                }
                else
                {
                    status = false;
                }
            }

        }
    }
}
