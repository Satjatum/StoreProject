using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreProject
{
    public partial class FrmProductCreate : Form
    {
        //สร้างตัวแปรเก็บรูปที่แปลงเป็น Binary/Byte Array เอาไว้บันทึกลง DB
        byte[] proImage;
        public FrmProductCreate()
        {
            InitializeComponent();
        }

        //สร้าง Method แปลงรูปเป็น Binary/Byte Array 
        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void btProImage_Click(object sender, EventArgs e)
        {
            //เปิด File Dialog  ให้เลือกรูปโดยฟิวเตอร์เฉพาะไฟล์ jpg/png
            //แล้วนำรูปทื่เลือกไปแสดงที่ pcbProImage
            //แล้วแปลงเป็นร Binary/Byte เก็บในตัวแปรเพื่อเอาไว้บันทึก DB
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Filter = "Image File (*.jpg;*.png)|*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกไปแสดงที่ pcbProImage
                pcbProImage.Image = Image.FromFile(openFileDialog.FileName);
                //ตรวจสอบ Format ของรูป แล้วส่งรูปไปแปลงเป็น Binary/Byte เก็บในตัวแปร
                if (pcbProImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    proImage = convertImageToByteArray(pcbProImage.Image, ImageFormat.Jpeg);

                }
                else
                {
                    proImage = convertImageToByteArray(pcbProImage.Image, ImageFormat.Png);
                }
            }
        }

        private void tbProPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // อนุญาตให้กดปุ่ม backspace
            if (char.IsControl(e.KeyChar))
                return;

            // ตรวจสอบว่าเป็นตัวเลขหรือไม่
            if (char.IsDigit(e.KeyChar))
                return;

            // ตรวจสอบว่าเป็นจุดทศนิยมหรือไม่ และยังไม่มีจุดใน textbox
            if (e.KeyChar == '.' && !((TextBox)sender).Text.Contains("."))
                return;

            // ถ้าไม่ใช่ตัวเลข หรือ จุดที่มากกว่าหนึ่งจุด ให้ยกเลิกการพิมพ์
            e.Handled = true;

        }

        // สร้าง Method แสดงข้อความเตือน
        private void ShowWarningMSG(string msg)
        {

            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        private void btSave_Click(object sender, EventArgs e)
        {
            //Validate UI แสดลงข้อความเตือนด้วยเมือ Validate แล้วก็เอาข้อมูลไปบันทึกลง DB 
            //พอบันทึกเสร็จแสดงข้อความบอกผู้ใช้ และปิดหน้าจอ FrmProductCreate และกลับไปยังหน้าจอ FrmProductShow
            if (proImage == null)
            {
                ShowWarningMSG("กรุณาเลือกรูปภาพ");
            }
            else if (tbProName.Text.Length == 0)
            {
                ShowWarningMSG("กรุณาป้อนชื่อสินค้า");
            }
            else if (tbProPrice.Text.Length == 0)
            {
                ShowWarningMSG("กรุณาป้อนราคาสินค้า");
            }
            else if (nudProQuan.Value == 0)
            {
                ShowWarningMSG("จำนวนสินค้าต้องมากกว่า 0");
            }
            else if (tbProUnit.Text.Length == 0)
            {
                ShowWarningMSG("ป้อนหน่วยสินค้าด้วย");
            }
            else
            {
                //บันทึกลง DB -> แสดง MSG --> ปิดหน้าจอ --> กลับไปยัง FrmProductShow
                string connectionString = @"Server=SATJATUM\SQLEXPRESS01;Database=store_db;Trusted_Connection=True";

                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(); //ใช้กับ Insert/update/delete

                        //คำสั่ง SQL
                        String strSql = "INSERT INTO product_tb (proName,proPrice,proQuan,proUnit,proStatus,proImage,createAt,updateAt)" +
                                        "VALUES (@proName,@proPrice,@proQuan,@proUnit,@proStatus,@proImage,@createAt,@updateAt)";

                        //กำหนดค่าให้กับ SQL Parameter และสั่งให้คำสั่ง SQL ทำงาน
                        using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@proName", SqlDbType.NVarChar, 300).Value = tbProName.Text;
                            sqlCommand.Parameters.Add("@proPrice", SqlDbType.Float).Value = float.Parse(tbProPrice.Text);   
                            sqlCommand.Parameters.Add("@proQuan", SqlDbType.Int).Value = int.Parse(nudProQuan.Value.ToString());
                            sqlCommand.Parameters.Add("@proUnit", SqlDbType.NVarChar,50).Value = tbProUnit.Text;
                            if(rdoProStatusOn.Checked == true)
                            {
                                sqlCommand.Parameters.Add("@proStatus", SqlDbType.NVarChar, 50).Value = "พร้อมขาย";
                            } else
                            {
                                sqlCommand.Parameters.Add("@proStatus", SqlDbType.NVarChar, 50).Value = "ไม่พร้อมขาย";
                            }
                            sqlCommand.Parameters.Add("@proImage", SqlDbType.Image).Value = proImage;
                            sqlCommand.Parameters.Add("@createAt", SqlDbType.Date).Value = DateTime.Now.Date;
                            sqlCommand.Parameters.Add("@updateAt", SqlDbType.Date).Value = DateTime.Now.Date;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

           
                            MessageBox.Show("บันทึกเรียบร้อย","ผลการทำงาน",MessageBoxButtons.OK,MessageBoxIcon.Information);
                            
                            this.Close();
                        }
                    
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            //เคลียหน้าจอ
            proImage = null;
            tbProName.Clear();
            tbProPrice.Clear();
            nudProQuan.Value = 0;
            tbProUnit.Clear();
            pcbProImage.Image = null;
        }
    }
}
