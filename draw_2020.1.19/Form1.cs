using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging; // ColorMatrix
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace draw_2020_1_19 {
    public partial class Form1 : Form {
        int w, h, pen_width = 1, backupIndex = 0, top, left, cut_width, cut_height, tools = -3;
        Bitmap tmpImg, cutOriginMap, tmpCutMap;
        float r = 1, gv = 1, b = 1, a = 1;
        Pen p, penDash = new Pen(Color.DarkSlateGray);
        Color penColor = Color.Black, brushColor = Color.Black;
        bool F = false, P = false, P2 = false;
        Brush brush = new SolidBrush(Color.Black);
        List<Bitmap> backBitmap = new List<Bitmap>();
        Point cut_Point, movePoint = new Point(-1, -1), oldXY; // 滑鼠位置

        public Form1() {
            InitializeComponent();
            DoubleBuffered = true;
            p = new Pen(penColor, pen_width);
            復原ToolStripMenuItem1.Enabled = false;
            取消復原ToolStripMenuItem1.Enabled = false;
            剪下ToolStripMenuItem.Enabled = false;
            複製ToolStripMenuItem.Enabled = false;
            貼上ToolStripMenuItem.Enabled = false;
            無填滿ToolStripMenuItem.Checked = true;
            this.ClientSize = new Size(675, 400);

            penDash.DashStyle = DashStyle.Dash;
            toolStripStatusLabel1.Text = "Width: " + w.ToString() + ", Height: " + h.ToString();
            toolStripStatusLabel3.Text = "Pen:" + penColor.ToString();
            toolStripStatusLabel4.Text = "Brush:" + brushColor.ToString();
        }

        private void 開啟舊檔ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) { // 開啟影像檔
                String input = openFileDialog1.FileName;
                tmpImg = (Bitmap)Image.FromFile(input); // 產生一個Image物件
                w = tmpImg.Width;
                h = tmpImg.Height;
                ChangeSize();

                pictureBox1.Image = tmpImg;
                pictureBox1.Refresh(); // 要求重畫
                SaveBackMap();
                toolStripStatusLabel1.Text = "Width: " + w.ToString() + ", Height: " + h.ToString();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void 開啟新檔ToolStripMenuItem_Click(object sender, EventArgs e) {
            Form2 x = new Form2();
            x.TopMost = true;  //移到最上層
            x.Text = "設定畫布的寬與高"; //Form2 的標題
            x.ShowDialog();    // Show Form2
            w = x.getWidth();
            h = x.getHeight();
            if (w != -1) {
                tmpImg = new Bitmap(w, h);
                pictureBox1.Image = tmpImg;
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                g.Clear(Color.White);
                pictureBox1.Refresh(); // 要求重畫
                ChangeSize();
                SaveBackMap();
                toolStripStatusLabel1.Text = "Width: " + w.ToString() + ", Height: " + h.ToString();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void 儲存ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                    String output = saveFileDialog1.FileName;
                    pictureBox1.Image.Save(output + ".jpg");
                    tmpImg = (Bitmap)pictureBox1.Image;
                }
            }
        }

        private void 灰階ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                ColorMatrix cm = new ColorMatrix(
                       new float[][]{ new float[]{0.33f, 0.33f, 0.33f, 0, 0},
                                  new float[]{0.33f, 0.33f, 0.33f, 0, 0},
                                  new float[]{0.33f, 0.33f, 0.33f, 0, 0},
                                  new float[]{  0,    0,    0,  1, 0},
                                  new float[]{  0,    0,    0,  0, 1}});
                pictureBox1.Image = ConvertCM(pictureBox1.Image, cm);
                SaveBackMap();
            }
        }

        private void 負片ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                ColorMatrix cm = new ColorMatrix(
                   new float[][]{ new float[]{ -1f,    0,    0,  0, 0},
                                  new float[]{  0,   -1f,    0,  0, 0},
                                  new float[]{  0,    0,   -1f,  0, 0},
                                  new float[]{  0,    0,    0,  1, 0},
                                  new float[]{  1,    1,    1,  0, 1}});
                pictureBox1.Image = ConvertCM(pictureBox1.Image, cm);
                SaveBackMap();
            }
        }

        public Bitmap ConvertCM(Image image, ColorMatrix cm) {
            Bitmap dest = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(dest); // 從點陣圖 建立 新的畫布
            // cm 定義含有 RGBA 空間座標的 5 x 5 矩陣
            // (R, G, B, A, 1) 乘上 此矩陣
            ImageAttributes ia = new ImageAttributes(); // ImageAttributes 類別的多個方法會使用色彩矩陣來調整影像色彩
            ia.SetColorMatrix(cm); // 設定預設分類的色彩調整矩陣。
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
            g.Dispose(); //清掉畫布與點陣圖變數的連結

            tmpImg = (Bitmap)pictureBox1.Image.Clone();
            復原ToolStripMenuItem1.Enabled = true;
            return dest;
        }

        private void 自訂ToolStripMenuItem_Click(object sender, EventArgs e) {
            Form3 rgba = new Form3();
            rgba.TopMost = true;
            rgba.ShowDialog();
            r = rgba.getR();
            gv = rgba.getG();
            b = rgba.getB();
            a = rgba.getA();
            float[][] cmArray1 ={
                  new float[] {r, 0, 0, 0, 0},
                  new float[] {0, gv, 0, 0, 0},
                  new float[] {0, 0, b, 0, 0},
                  new float[] {0, 0, 0, a, 0},
                  new float[] {0, 0, 0, 0, 1}
               };
            ColorMatrix cm1 = new ColorMatrix(cmArray1);
            ImageAttributes ia1 = new ImageAttributes();
            ia1.SetColorMatrix(cm1,ColorMatrixFlag.Default,ColorAdjustType.Bitmap);
            Rectangle rectDest = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);// 繪出透明的背景影像
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.DrawImage(pictureBox1.Image, rectDest, 0, 0, pictureBox1.Width, pictureBox1.Height, GraphicsUnit.Pixel, ia1);
            pictureBox1.Refresh();
            SaveBackMap();
        }

        private void 增亮ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                ColorMatrix cm = new ColorMatrix(
                   new float[][]{ new float[]{  1.1f,    0,    0,  0, 0},
                                  new float[]{  0,    1.1f,    0,  0, 0},
                                  new float[]{  0,    0,    1.1f,  0, 0},
                                  new float[]{  0,    0,    0,  1, 0},
                                  new float[]{  0,    0,    0,  0, 1}});
                pictureBox1.Image = ConvertCM(pictureBox1.Image, cm);
                SaveBackMap();
            }
        }

        private void 調暗ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                ColorMatrix cm = new ColorMatrix(
                   new float[][]{ new float[]{  0.9f,    0,    0,  0, 0},
                                  new float[]{  0,    0.9f,    0,  0, 0},
                                  new float[]{  0,    0,    0.9f,  0, 0},
                                  new float[]{  0,    0,    0,  1, 0},
                                  new float[]{  0,    0,    0,  0, 1}});
                pictureBox1.Image = ConvertCM(pictureBox1.Image, cm);
                SaveBackMap();
            }
        }

        private void 一半ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                Bitmap p = new Bitmap(w / 2, h / 2);
                for (int i = 0; i < w; i += 2)
                    for (int j = 0; j < h; j += 2) {
                        if ((i / 2 < w) && (j / 2 < h))
                            p.SetPixel(i / 2, j / 2, tmpImg.GetPixel(i, j));
                    }
                pictureBox1.Image = (Bitmap)p.Clone();
                w = p.Width;
                h = p.Height;
                pictureBox1.Refresh();
                toolStripStatusLabel1.Text = "Width: " + w.ToString() + ", Height: " + h.ToString();
                SaveBackMap();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void 兩倍ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                Bitmap p = new Bitmap(w * 2, h * 2);
                tmpImg = (Bitmap)pictureBox1.Image;
                for (int i = 0; i < w; i++)
                    for (int j = 0; j < h; j++) {
                        Color c = tmpImg.GetPixel(i, j);
                        for (int ii = 0; ii < 2; ii++)
                            for (int jj = 0; jj < 2; jj++)
                                p.SetPixel(i * 2 + ii, j * 2 + jj, c); //垂直與水平方向都重複畫兩遍
                    }
                pictureBox1.Image = (Bitmap)p.Clone();
                w = p.Width;
                h = p.Height;
                pictureBox1.Refresh();
                toolStripStatusLabel1.Text = "Width: " + w.ToString() + ", Height: " + h.ToString();
                ChangeSize();
                SaveBackMap();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void 點ToolStripMenuItem_Click(object sender, EventArgs e) {
            tools = 0;
        }

        private void 直線ToolStripMenuItem_Click(object sender, EventArgs e) {
            tools = 1;
        }

        private void 矩形ToolStripMenuItem_Click(object sender, EventArgs e) {
            tools = 2;
        }

        private void 圓ToolStripMenuItem_Click(object sender, EventArgs e) {
            tools = 3;
        }

        private void 顏色ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (colorDialog1.ShowDialog() == DialogResult.OK) {
                penColor = colorDialog1.Color;
                p = new Pen(penColor, pen_width);
                toolStripStatusLabel3.Text = "Pen: " + penColor.ToString();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (pictureBox1.Image != null) {
                Bitmap temp = (Bitmap)backBitmap[backupIndex].Clone();
                pictureBox1.Image = temp;
                pictureBox1.Refresh();

                if (tools == 4) {
                    if (movePoint.X != -1 && e.X > movePoint.X && e.X < movePoint.X + cut_width && e.Y > movePoint.Y && e.Y < movePoint.Y + cut_height) {
                        tools = -1;
                        top = e.Y - movePoint.Y;
                        left = e.X - movePoint.X;
                    } else {
                        cut_Point = movePoint = e.Location;
                    }
                } else if (tools == -1) {
                    if (P) {
                        movePoint.X = 0;
                        movePoint.Y = 0;
                        if (e.X > movePoint.X && e.X < movePoint.X + cut_width && e.Y > movePoint.Y && e.Y < movePoint.Y + cut_height) {
                            top = e.Y - movePoint.Y;
                            left = e.X - movePoint.X;
                            P = false;
                            P2 = true;
                        } else {
                            tools = -2;
                            P2 = false;
                        }
                    } else {
                        if (e.X > movePoint.X && e.X < movePoint.X + cut_width && e.Y > movePoint.Y && e.Y < movePoint.Y + cut_height) {
                            movePoint = e.Location;
                        } else {
                            tools = -2;
                            剪下ToolStripMenuItem.Enabled = false;
                            複製ToolStripMenuItem.Enabled = false;
                        }
                    }
                }
                oldXY = e.Location;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (pictureBox1.Image != null) {
                if ((e.X < w) && (e.Y < h) && (e.X > 0) && (e.Y > 0)) {
                    toolStripStatusLabel2.Text = e.Location.ToString();
                } else {
                    toolStripStatusLabel2.Text = "{,}";
                }

                if (e.Button == MouseButtons.Left && tools >= 0 && tools < 5) {
                    Bitmap tempImg = (Bitmap)backBitmap[backupIndex].Clone();
                    Graphics g = Graphics.FromImage(pictureBox1.Image);
                    Graphics gg = Graphics.FromImage(tempImg);
                    switch (tools) {
                        case 0:
                            g.DrawLine(p, oldXY.X, oldXY.Y, e.X, e.Y);
                            oldXY = e.Location;
                            tempImg = (Bitmap)pictureBox1.Image.Clone();
                            break;
                        case 1:
                            gg.DrawLine(p, oldXY.X, oldXY.Y, e.X, e.Y);
                            break;
                        case 2:
                            if (F) gg.FillRectangle(brush, oldXY.X, oldXY.Y, e.X - oldXY.X, e.Y - oldXY.Y);
                            gg.DrawRectangle(p, oldXY.X, oldXY.Y, e.X - oldXY.X, e.Y - oldXY.Y);
                            break;
                        case 3:
                            if (F) gg.FillEllipse(brush, oldXY.X, oldXY.Y, e.X - oldXY.X, e.Y - oldXY.Y);
                            gg.DrawEllipse(p, oldXY.X, oldXY.Y, e.X - oldXY.X, e.Y - oldXY.Y);
                            break;
                        case 4:
                            gg.DrawRectangle(penDash, oldXY.X, oldXY.Y, e.X - oldXY.X, e.Y - oldXY.Y);
                            break;
                        default:
                            break;
                    }
                    pictureBox1.Image = tempImg;
                    pictureBox1.Refresh();
                }
            }
            GC.Collect();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (pictureBox1.Image != null) {
                if (backupIndex < backBitmap.Count - 1) {
                    backBitmap.RemoveRange(backupIndex + 1, backBitmap.Count - backupIndex - 1);
                    取消復原ToolStripMenuItem1.Enabled = false;
                }
                if (tools == -1) {
                    tmpImg = (Bitmap)cutOriginMap.Clone();
                    Graphics g = Graphics.FromImage(tmpImg);
                    g.DrawImage(tmpImg, 0, 0, tmpImg.Width, tmpImg.Height);
                    Rectangle rectDest, rectSRC;
                    rectDest = new Rectangle(e.X - left, e.Y - top, cut_width, cut_height);
                    if (P2) {
                        rectSRC = new Rectangle(0, 0, cut_width, cut_height);
                        g.DrawImage(tmpCutMap, rectDest, rectSRC, GraphicsUnit.Pixel);
                    } else {
                        rectSRC = new Rectangle(cut_Point.X, cut_Point.Y, cut_width, cut_height);
                        g.FillRectangle(new SolidBrush(Color.White), rectSRC);
                        g.DrawImage(cutOriginMap, rectDest, rectSRC, GraphicsUnit.Pixel);
                    }

                    backupIndex++;
                    backBitmap.Add((Bitmap)tmpImg.Clone());
                    g.DrawRectangle(penDash, rectDest); //框線
                    pictureBox1.Image = tmpImg;
                    pictureBox1.Refresh();
                    movePoint.X = e.X - left;
                    movePoint.Y = e.Y - top;
                }
                if (tools == 4) {
                    cutOriginMap = (Bitmap)backBitmap[backupIndex].Clone();
                    cut_height = e.Location.Y - movePoint.Y;
                    cut_width = e.Location.X - movePoint.X;
                    if (e.Location.X != movePoint.X && e.Location.Y != movePoint.Y) {
                        剪下ToolStripMenuItem.Enabled = 複製ToolStripMenuItem.Enabled = true;
                    } else {
                        剪下ToolStripMenuItem.Enabled = 複製ToolStripMenuItem.Enabled = false;
                    }
                } else if (tools >= 0) {
                    tmpImg = (Bitmap)pictureBox1.Image.Clone();
                    backupIndex++;
                    backBitmap.Add(tmpImg);
                    取消復原ToolStripMenuItem1.Enabled = false;
                }
                if (backBitmap.Count == 1) {
                    復原ToolStripMenuItem1.Enabled = false;
                } else {
                    復原ToolStripMenuItem1.Enabled = true;
                }
            }
        }

        private void MenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (tools == 4) {
                if (backupIndex < backBitmap.Count - 1) {
                    cutOriginMap = (Bitmap)backBitmap[backupIndex].Clone();
                }
            }
            if (backBitmap.Count > 0) {
                pictureBox1.Image = (Bitmap)backBitmap[backupIndex].Clone();
                pictureBox1.Refresh();
            }
            P2 = false;
        }

        private void 選取ToolStripMenuItem_Click(object sender, EventArgs e) {
            剪下ToolStripMenuItem.Enabled = false;
            複製ToolStripMenuItem.Enabled = false;
            貼上ToolStripMenuItem.Enabled = false;
            tools = 4;
            P = false;
        }

        private void 剪下ToolStripMenuItem_Click(object sender, EventArgs e) {
            貼上ToolStripMenuItem.Enabled = true;
            剪下ToolStripMenuItem.Enabled = false;
            複製ToolStripMenuItem.Enabled = false;
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                if (backupIndex < backBitmap.Count - 1) {
                    backBitmap.RemoveRange(backupIndex + 1, backBitmap.Count - backupIndex - 1);
                }
                Graphics g = Graphics.FromImage(tmpImg);
                Graphics gg = Graphics.FromImage(cutOriginMap);

                g.DrawImage(tmpImg, 0, 0, tmpImg.Width, tmpImg.Height);

                tmpCutMap = new Bitmap(cut_width, cut_height); //截圖存到tmpCutMap
                Graphics ggg = Graphics.FromImage(tmpCutMap);
                ggg.DrawImage(tmpImg, new Rectangle(0, 0, cut_width, cut_height), new Rectangle(cut_Point.X, cut_Point.Y, cut_width, cut_height), GraphicsUnit.Pixel);

                g.FillRectangle(new SolidBrush(Color.White), new Rectangle(cut_Point.X, cut_Point.Y, cut_width, cut_height));
                gg.FillRectangle(new SolidBrush(Color.White), new Rectangle(cut_Point.X, cut_Point.Y, cut_width, cut_height));

                backupIndex++;
                backBitmap.Add((Bitmap)tmpImg.Clone());
                pictureBox1.Image = (Bitmap)tmpImg.Clone();
                pictureBox1.Refresh();
                tools = -2;
            }
        }

        private void 複製ToolStripMenuItem_Click(object sender, EventArgs e) {
            貼上ToolStripMenuItem.Enabled = true;
            剪下ToolStripMenuItem.Enabled = false;
            複製ToolStripMenuItem.Enabled = false;
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                tmpCutMap = new Bitmap(cut_width, cut_height);
                Graphics g = Graphics.FromImage(tmpCutMap);
                g.DrawImage(tmpImg, new Rectangle(0, 0, cut_width, cut_height), new Rectangle(cut_Point.X, cut_Point.Y, cut_width, cut_height), GraphicsUnit.Pixel);
                tools = -2;
            }
        }

        private void 貼上ToolStripMenuItem_Click(object sender, EventArgs e) {
            剪下ToolStripMenuItem.Enabled = false;
            複製ToolStripMenuItem.Enabled = false;
            P = true;
            tools = -1;

            tmpImg = (Bitmap)backBitmap[backupIndex].Clone();
            cutOriginMap = (Bitmap)backBitmap[backupIndex].Clone();
            Graphics g = Graphics.FromImage(tmpImg);
            Rectangle rectDest = new Rectangle(0, 0, cut_width, cut_height);
            Rectangle rectSRC = new Rectangle(0, 0, cut_width, cut_height);
            g.DrawImage(tmpCutMap, rectDest, rectSRC, GraphicsUnit.Pixel);
            backupIndex++;
            backBitmap.Add((Bitmap)tmpImg.Clone());
            g.DrawRectangle(penDash, rectDest); //框線

            pictureBox1.Image = (Bitmap)tmpImg.Clone();
            pictureBox1.Refresh();
        }

        private void 向右旋轉ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                tmpImg.RotateFlip(RotateFlipType.Rotate90FlipNone);
                pictureBox1.Image = tmpImg;
                int tmp = w;
                w = h;
                h = tmp;
                toolStripStatusLabel1.Text = "Width: " + w.ToString() + ", Height: " + h.ToString();
                ChangeSize();
                this.Invalidate();
                SaveBackMap();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void 水平翻轉ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                tmpImg.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox1.Image = tmpImg;
                this.Invalidate();
                SaveBackMap();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void 垂直翻轉ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                tmpImg = (Bitmap)pictureBox1.Image.Clone();
                tmpImg.RotateFlip(RotateFlipType.RotateNoneFlipY);
                pictureBox1.Image = tmpImg;
                this.Invalidate();
                SaveBackMap();
                剪下ToolStripMenuItem.Enabled = false;
                複製ToolStripMenuItem.Enabled = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (pictureBox1.Image != null) {
                Form4 x = new Form4();
                x.TopMost = true;  //移到最上層
                x.Text = "檔案即將關閉"; //Form4 的標題
                x.ShowDialog();    // Show Form4

                if (x.getButton()) {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                        String output = saveFileDialog1.FileName;
                        pictureBox1.Image.Save(output + ".jpg");
                        tmpImg = (Bitmap)pictureBox1.Image;
                    }
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) {
            pen_width = 1;
            toolStripMenuItem2.Checked = true;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            p = new Pen(penColor, pen_width);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e) {
            pen_width = 2;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = true;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            p = new Pen(penColor, pen_width);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e) {
            pen_width = 3;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = true;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            p = new Pen(penColor, pen_width);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e) {
            pen_width = 4;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = true;
            toolStripMenuItem6.Checked = false;
            p = new Pen(penColor, pen_width);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e) {
            pen_width = 5;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = true;
            p = new Pen(penColor, pen_width);
        }

        private void 顏色ToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (colorDialog2.ShowDialog() == DialogResult.OK) {
                brushColor = colorDialog2.Color;
                brush = new SolidBrush(brushColor);
                toolStripStatusLabel4.Text = "Brush: " + brushColor.ToString();
            }
        }

        private void 填滿ToolStripMenuItem_Click(object sender, EventArgs e) {
            填滿ToolStripMenuItem.Checked = true;
            無填滿ToolStripMenuItem.Checked = false;
            F = true;
        }

        private void 無填滿ToolStripMenuItem_Click(object sender, EventArgs e) {
            填滿ToolStripMenuItem.Checked = false;
            無填滿ToolStripMenuItem.Checked = true;
            F = false;
        }

        private void 復原ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupIndex == 1) {
                復原ToolStripMenuItem1.Enabled = false;
            }
            取消復原ToolStripMenuItem1.Enabled = true;
            pictureBox1.Image = (Bitmap)backBitmap[--backupIndex].Clone();
            pictureBox1.Refresh();
        }

        private void 取消復原ToolStripMenuItem_Click(object sender, EventArgs e) {
            復原ToolStripMenuItem1.Enabled = true;
            if (backupIndex == backBitmap.Count - 2) {
                取消復原ToolStripMenuItem1.Enabled = false;
            }
            pictureBox1.Image = (Bitmap)backBitmap[++backupIndex].Clone();
            pictureBox1.Refresh();
        }

        private void SaveBackMap() {
            if (backupIndex < backBitmap.Count - 1) {
                backBitmap.RemoveRange(backupIndex + 1, backBitmap.Count - backupIndex - 1);
            }
            if (backBitmap.Count != 0) { //不是第一張
                backupIndex++;
                復原ToolStripMenuItem1.Enabled = true;
            }
            backBitmap.Add((Bitmap)pictureBox1.Image.Clone());
            if (backupIndex == backBitmap.Count - 1) {
                取消復原ToolStripMenuItem1.Enabled = false;
            }
            tools = -3;
        }

        private void ChangeSize() {
            if ((this.ClientSize.Width < w) || (this.ClientSize.Height < h)) {
                if (w < 675) {
                    this.ClientSize = new Size(675, h + 56);
                } else {
                    this.ClientSize = new Size(w, h + 56);
                }
            }
        }
    }
}
