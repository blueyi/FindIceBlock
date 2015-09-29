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
            string picNameTest = @"D:\blueyi\Documents\Visual Studio 2013\Projects\FindIceBlock\bin\Debug\Temp\Test_pic.jpg";

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
            arrToFile("board.txt", resultBoard, sWidth, sHeight);

            if (icenum > 0)
                iceBlockNum = findIce(resultBoard, resultSum, sWidth, sHeight, icenum);

            arrToFile("resultBoard.txt", resultBoard, sWidth, sHeight);

            if (iceBlockNum > 0)
            {
                textBox1.AppendText("--------------------------------------\n");
                for (int i = 0; i <= iceBlockNum; i++)
                {
                    textBox1.AppendText(i.ToString() + ": " + (resultSum[i]).ToString() + "\n");
                }
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
            int[] currentCheckedIceIdx;   //用于存放当前检查过的像素索引
            int arrLength = width * height;
            int[] arrLine = new int[arrLength];  //存放一维化之后的数组
            int lastPoint = 0; //上一块冰的起点

            //将像素线性化
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    arrLine[j * width + i] = resultArr[i, j];
                }
            }

            while (allIceNumHere != 0)
            {
                int idxUp = 0;  //向上扫描的起始坐标
                int idxDown = 0;  //向下扫描的起始坐标
                int currentOrderStart = 0;  //记录当前序号冰块的检查起点

                currentCheckedIceIdx = new int[allIceNum];  

                iceOrder++;
                iceOrderSum = 0;  //当前冰块序号对应的所有冰数量

                //找到需要标记的冰,即像素为1的点
                findFirstPoint(ref lastPoint, arrLine, arrLength);
//                isAllCheckedInCurrentIce(resultArr, 1, ref idxUpX, ref idxUpY, ref idxDownX, ref idxDownY, width, height);
//                idxDownX = idxUpX;
//                idxDownY = idxUpY;
                idxDown = idxUp = lastPoint;

                //确定当前冰块的初始扫描行
                markIceLine(arrLine, iceOrder, ref iceOrderSum, idxUp, currentCheckedIceIdx, width, ref allIceNumHere);

                //检查是否当前连通块的冰全部标记完成
                while (!isAllCheckedInCurrentIceBlock(currentCheckedIceIdx, arrLine, ref currentOrderStart, ref idxUp, ref idxDown, width, height, iceOrderSum))
                {
                    
                    //重要的边界------
                        while ((idxUp >= 0) && (arrLine[idxUp] == 1) && (arrLine[idxUp] != iceOrder))
                        {
//                            markIceLine(resultArr, iceOrder, ref iceOrderSum, ref idxUpX, ref idxUpY, width, ref allIceNumHere);
//                            idxUpY--;
                            markIceLine(arrLine, iceOrder, ref iceOrderSum, idxUp, currentCheckedIceIdx, width, ref allIceNumHere);
                            idxUp = idxUp - width + idxUp % width;
                        }

                    //重要的边界------
                        while ((idxDown / width < height) && (arrLine[idxDown] == 1) && (arrLine[idxDown] != iceOrder))
                        {
                            markIceLine(arrLine, iceOrder, ref iceOrderSum, idxDown, currentCheckedIceIdx, width, ref allIceNumHere);
                            idxDown = idxDown + width + idxDown % width;
                           // markIceLine(resultArr, iceOrder, ref iceOrderSum, ref idxDownX, ref idxDownY, width, ref allIceNumHere);
                           // idxDownY++;
                        }
                }
                resultSum[iceOrder] = iceOrderSum;
            }

            int idxX = 0;
            int idxY = 0;
            for (int idx = 0; idx < arrLength; idx++)
            {
                idxX = idx % width;
                idxY = idx / width;
                resultArr[idxX, idxY] = arrLine[idx];
            }

                return iceOrder;
        }

        //标记当前行中所有需要标记的冰块
        public void markIceLine(int[] arrLine, int iceOrder, ref int iceOrderSum, int lastPoint, int[] currentCheckedIceIdx, int width, ref int allIceNumHere)
        {
            int leftX = lastPoint;
            int rightX = lastPoint + 1;
            while (true)
            {
                arrLine[leftX] = iceOrder;
                currentCheckedIceIdx[iceOrderSum] = leftX;  //记录当前冰块坐标
                iceOrderSum++;
                allIceNumHere--;
                leftX--;

                    //重要的边界------
                //满足以下条件退出：超出图像左边界, 该位置不是冰，该文件是标记过的冰
                // if ((leftX < 0) || (((leftX + 1) % width) == 0) || (arrLine[leftX] == 0) || (arrLine[leftX] == iceOrder))
                if ((leftX < 0) || (((leftX + 1) % width) == 0) || (arrLine[leftX] != 1))
                    break;
            }

            while (true)
            {
                    //重要的边界------
                //满足以下条件退出：超出图像左边界, 该位置不是冰，该文件是标记过的冰
                if ((rightX > arrLine.Length) || ((rightX % width) == 0) || (arrLine[rightX] != 1))  //如果向右已经到达图像右边界，则直接退出
                    break;

                arrLine[rightX] = iceOrder;
                currentCheckedIceIdx[iceOrderSum] = rightX;  //记录当前冰块坐标
                iceOrderSum++;
                allIceNumHere--;
                rightX++;

                //      if (((rightX % width) >= width) || (arrLine[rightX] == 0) || (arrLine[rightX] == iceOrder))
                //         break;
            }
 
        }

        public int idxConvert(int x, int y, int width)
        {
            return (y * width + x);
        }

        //每块冰第一次被扫描到时，确定出它的初始位置
        public void findFirstPoint(ref int lastPoint, int[] arrLine, int arrLength)
        {
            int i = lastPoint;
            while (i < arrLength)
            {
                if (arrLine[i] == 1)
                    break;
                i++;
            }
            lastPoint = i;
        }

        //判断是否当前冰块中的所有像素都已经被扫描过, 如果不是，则确定出下一次需要扫描的起点
        public bool isAllCheckedInCurrentIceBlock(int[] currentCheckedIceIdx, int[] arrLine, ref int currentOrderStart, ref int idxUp,  ref int idxDown,int width, int height, int iceOrderSum)
        {
            bool checkDone = true;
            int x, y;
            int idxX, idxY;
            idxX = idxY = 0;
            int idx = currentOrderStart;

            for (; idx < iceOrderSum; idx++)
            {
                x = currentCheckedIceIdx[idx] % width;
                y = currentCheckedIceIdx[idx] / width;
                if ((x - 1 >= 0) && arrLine[x - 1 + y * width] == 1)
                {
                    checkDone = false;
                    idxX = x - 1;
                    idxY = y;
                    break;
                }

                if ((x + 1 < width) && arrLine[x + 1 + y * width] == 1)
                {
                    checkDone = false;
                    idxX = x + 1;
                    idxY = y;
                    break;
                }

                if ((y - 1 >= 0) && arrLine[x + (y - 1) * width] == 1)
                {
                    checkDone = false;
                    idxX = x;
                    idxY = y - 1;
                    break;
                }

                if ((y + 1 < height) && arrLine[x + (y + 1) * width] == 1)
                {
                    checkDone = false;
                    idxX = x;
                    idxY = y + 1;
                    break;
                }

                if ((x - 1 >= 0) && (y - 1 >= 0) && arrLine[x - 1 + (y - 1) * width] == 1)
                {
                    checkDone = false;
                    idxX = x - 1;
                    idxY = y - 1;
                    break;
                }

                if ((x + 1 < width) && (y - 1 >= 0) && arrLine[x + 1 + (y - 1) * width] == 1)
                {
                    checkDone = false;
                    idxX = x + 1;
                    idxY = y - 1;
                    break;
                }

                if ((x + 1 < width) && (y + 1 < height) && arrLine[x + 1 + (y + 1) * width] == 1)
                {
                    checkDone = false;
                    idxX = x + 1;
                    idxY = y + 1;
                    break;
                }

                if ((x - 1 >= 0) && (y + 1 < height) && arrLine[x - 1 + (y + 1) * width] == 1)
                {
                    checkDone = false;
                    idxX = x - 1;
                    idxY = y + 1;
                    break;
                }

            }
            currentOrderStart = idx + 1;  //记录当前冰块中下一次的起点
            idxDown = idxUp = idxY * width + idxX;
            if (idx == iceOrderSum)
            {
                currentOrderStart = 0;
                idxDown = idxUp = 0;
            }

            return checkDone;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            picCalculate();
        }
    }
}
