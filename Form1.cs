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
using System.Drawing.Imaging;

namespace NewMethod
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public void arrToFile(string fileName, int[,] arr, int width, int height)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            string line = "";

            for (int j = 0; j < height; j++)
            {
                line = "";
                for (int i = 0; i < width; i++)
                {
                    line += (arr[i, j]).ToString();
                }
                sw.WriteLine(line);
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        public void picCalculate()
        {

            //---debug-----
            string picNameTest = @"C:\Users\blueyi\Documents\Visual Studio 2013\Projects\Test\NewMethod\bin\Debug\Temp\Test_pic.jpg";

            Bitmap pic = new Bitmap(picNameTest);
            // Bitmap bgr = new Bitmap(longName3);

            int icenum = 0;  //所有冰块的总像素
            int sWidth = pic.Width;
            int sHeight = pic.Height;
            int[,] board = new int[sWidth, sHeight];
            // int a = sWidth * sHeight;

            Rectangle rec = new Rectangle(0, 0, sWidth, sHeight);
            BitmapData bd = pic.LockBits(rec, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = bd.Scan0;
            int bytes = bd.Stride * bd.Height;
            byte[] Trgbvalues;
            Trgbvalues = new byte[bytes];
            //Trgbvalues1 = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, Trgbvalues, 0, bytes);

            for (int j = 0; j < sHeight; j++)
            {
                for (int i = 0; i < sWidth; i++)
                {
                    if (Trgbvalues[j * bd.Stride + i * 3] == 255 && Trgbvalues[j * bd.Stride + i * 3 + 1] == 255 && Trgbvalues[j * bd.Stride + i * 3 + 2] == 255)
                    {
                        icenum++;
                        board[i, j] = 1;
                    }
                    else
                        board[i, j] = 0;
                }
            }

            int[,] resultBoard = new int[sWidth, sHeight];
            for (int j = 0; j < sHeight; j++)
            {
                for (int i = 0; i < sWidth; i++)
                {
                    resultBoard[i, j] = board[i, j];
                }
            }
            int[] resultSum = new int[icenum];

            int iceBlockNum = 0;

            //将数组写入到文件
//            arrToFile("board.txt", resultBoard, sWidth, sHeight);

            iceBlockNum = findIce(resultBoard, resultSum, sWidth, sHeight, icenum);

//            arrToFile("resultBoard.txt", resultBoard, sWidth, sHeight);

            for (int i = 0; i <= iceBlockNum; i++)
            {
                textBox1.AppendText(i.ToString() + ": " + (resultSum[i]).ToString() + "\n");
            }
                pic.Dispose();
        }

        //遍历并标记所有冰块
        public int findIce(int[,] resultArr, int[] resultSum, int width, int height, int allIceNum)
        {

            int allIceNumHere = allIceNum;  
//            bool[] isChecked = new bool[allIceNum]; //检查该冰点是否已被标记
            int iceOrder = 1;  //冰块顺序
            int iceOrderSum = 0;   //当前冰块序号对应的冰块大小
            while (allIceNumHere != 0)
            {
                int idxUpX = 0;
                int idxUpY = 0;
                int idxDownX = 0;
                int idxDownY = 0;

                iceOrder++;
                iceOrderSum = 0;  //当前冰块序号对应的所有冰数量

                //找到需要标记的冰,即像素为1的点
                isAllCheckedInCurrentIce(resultArr, 1, ref idxUpX, ref idxUpY, ref idxDownX, ref idxDownY, width, height);
                idxDownX = idxUpX;
                idxDownY = idxUpY;

                markIceLine(resultArr, iceOrder, ref iceOrderSum, ref idxUpX, ref idxUpY, width, ref allIceNumHere);

                //检查是否当前连通块的冰全部标记完成
                while (!isAllCheckedInCurrentIce(resultArr, iceOrder, ref idxUpX, ref idxUpY,  ref idxDownX, ref idxDownY, width, height))
                {
                    
                        while ((idxUpY > 0) && (resultArr[idxUpX, idxUpY] != 0) && (resultArr[idxUpX, idxUpY] != iceOrder))
                        {
                            markIceLine(resultArr, iceOrder, ref iceOrderSum, ref idxUpX, ref idxUpY, width, ref allIceNumHere);
                            idxUpY--;
                        }

                        while ((idxDownY < height) && (resultArr[idxDownX, idxDownY] != 0) && (resultArr[idxDownX, idxDownY] != iceOrder))
                        {
                            markIceLine(resultArr, iceOrder, ref iceOrderSum, ref idxDownX, ref idxDownY, width, ref allIceNumHere);
                            idxDownY++;
                        }
                }
                resultSum[iceOrder] = iceOrderSum;
            }
            return iceOrder;

        }

        //标记当前行中所有需要标记的冰块
        public void markIceLine(int[,] resultArr, int iceOrder, ref int iceOrderSum, ref int idxX, ref int idxY, int width, ref int allIceNumHere)
        {
            int leftX = idxX;
            int rightX = idxX + 1;
            while (true)
            {
                resultArr[leftX, idxY] = iceOrder;
                iceOrderSum++;
                allIceNumHere--;
                leftX--;
                if ((leftX < 0) || (resultArr[leftX, idxY] == 0) || (resultArr[leftX, idxY] == iceOrder))
                    break;
            }

            while (true)
            {
                if ((rightX >= width) || (resultArr[rightX, idxY] != 1))
                    break;
                resultArr[rightX, idxY] = iceOrder;
                iceOrderSum++;
                allIceNumHere--;
                rightX++;
                if ((rightX >= width) || (resultArr[rightX, idxY] == 0) || (resultArr[rightX, idxY] == iceOrder))
                    break;
            }
 
        }

        //判断是否当前冰块中的所有像素都已经被扫描过
        public bool isAllCheckedInCurrentIce(int[,] resultArr, int currentOrder, ref int idxUpX, ref int idxUpY,  ref int idxDownX, ref int idxDownY, int width, int height)
        {
            bool checkDone = true;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (resultArr[x, y] == currentOrder)
                    {
                        // 每块冰第一次被扫描到时确定出它的初始X和Y
                        if (currentOrder == 1)
                        {
                            idxUpX = idxDownX = x;
                            idxUpY = idxDownY = y;
                            checkDone = false;
                            break;
                        }

                        if ((x - 1 > 0) && resultArr[x - 1, y] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x - 1;
                            idxDownY = idxUpY = y;
                            break;
                        }

                        if ((x + 1 < width) && resultArr[x + 1, y] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x + 1;
                            idxDownY = idxUpY = y;
                            break;
                        }

                        if ((y - 1 > 0) && resultArr[x, y - 1] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x;
                            idxDownY = idxUpY = y - 1;
                            break;
                        }

                        if ((y + 1 < height) && resultArr[x, y + 1] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x;
                            idxDownY = idxUpY = y + 1;
                            break;
                        }

                        if ((x - 1 > 0) && (y - 1 > 0) && resultArr[x - 1, y - 1] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x - 1;
                            idxDownY = idxUpY = y - 1;
                            break;
                        }

                        if ((x + 1 < width) && (y - 1 > 0) && resultArr[x + 1, y - 1] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x + 1;
                            idxDownY = idxUpY = y - 1;
                            break;
                        }

                        if ((x + 1 < width) && (y + 1 < height) && resultArr[x + 1, y + 1] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x + 1;
                            idxDownY = idxUpY = y + 1;
                            break;
                        }

                        if ((x - 1 > 0) && (y + 1 < height) && resultArr[x - 1, y + 1] == 1)
                        {
                            checkDone = false;
                            idxDownX = idxUpX = x - 1;
                            idxDownY = idxUpY = y + 1;
                            break;
                        }

                    }

                }
                if (!checkDone)
                    break;
            }
            return checkDone;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            picCalculate();
        }
    }
}
