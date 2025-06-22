using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreProject
{
    public partial class FrmProductShow : Form
    {
        public FrmProductShow()
        {
            InitializeComponent();
        }

        // Method แปลง Binary เป็นรูป
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        // Method ดึงข้อมูลทั้งหมดจาก Product_tb มาแสดงที่ listView
        private void getAllProductToLv()
        {
            string connectionString = @"Server=SATJATUM\SQLEXPRESS01;Database=store_db;Trusted_Connection=True";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    //การทำงานกับตารางในฐานข้อมูล (SELECT, INSERT, UPDATE, DELETE)
                    //สร้างคำสั่ง Sql ดึงข้อมูลทั้งหมดในตาราง product_tb
                    string strSQL = "SELECT proId, proName,proPrice,proQuan, proUnit , proStatus, proImage From product_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQL เป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน DataTable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        //ตั้งค่า ListView
                        lvAllProduct.Items.Clear();
                        lvAllProduct.Columns.Clear();
                        lvAllProduct.FullRowSelect = true;
                        lvAllProduct.View = View.Details;

                        //ตั้งค่าการแสดงรูปใน View
                        if (lvAllProduct.SmallImageList == null)
                        {
                            lvAllProduct.SmallImageList = new ImageList();
                            lvAllProduct.SmallImageList.ImageSize = new Size(50, 50);
                            lvAllProduct.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvAllProduct.SmallImageList.Images.Clear();

                        //กำหนดรายละเอียด Colum ใน ListView
                        lvAllProduct.Columns.Add("รูปภาพ", 80, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("รหัสสินค้า", 100, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("ชื่อสินค้า", 150, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("ราคา", 100, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("จำนวน", 100, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("หน่วย", 100, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("สถานะ", 100, HorizontalAlignment.Left);


                        //Loop วนเข้าไปใน Datatable 
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); //สร้าง ITem เพื่อเก็บข้อมูลในแต่ละรายการ
                            //เอารูปใส่ใน Item
                            Image proImage = null;
                            if (dataRow["proImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["proImage"];
                                //แปลงข้อมูลรูปจากฐานข้อมูล Binary ให้เป็นรูป
                                proImage = convertByteArrayToImage(imgByte);
                            }
                            string imageKey = null;
                            if (proImage != null)
                            {
                                imageKey = $"pro_{dataRow["proId"]}";
                                lvAllProduct.SmallImageList.Images.Add(imageKey, proImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }
                            // เอาแต่ละรายการใส่ใน Item
                            item.SubItems.Add(dataRow["proId"].ToString());
                            item.SubItems.Add(dataRow["proName"].ToString());
                            item.SubItems.Add(dataRow["proPrice"].ToString());
                            item.SubItems.Add(dataRow["proQuan"].ToString());
                            item.SubItems.Add(dataRow["proUnit"].ToString());
                            item.SubItems.Add(dataRow["proStatus"].ToString());

                            //เอาข้อมูลใน Item 
                            lvAllProduct.Items.Add(item);
                        }
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }
        }


        private void FrmProductShow_Load(object sender, EventArgs e)
        {
            getAllProductToLv();
        }

        private void btnFrmProductCreate_Click(object sender, EventArgs e)
        {
            //เปิด FrmProductCreate แบบ Dialog/Popup    
            //สร้าง Object(ตัวแทน) ของ FrmProductCreate 
            FrmProductCreate frmProductCreate = new FrmProductCreate();
            //เปิดแบบปกติ
            //frmProductCreate.Show();
            //เปิดแบบ Dialog/
            frmProductCreate.ShowDialog();
            getAllProductToLv();
        }

        private void lvAllProduct_ItemActivate(object sender, EventArgs e)
        {
            //เปิดหน้าจอ FrmProductUpDel แบบ Dialog โดยจะส่ง ProId ของรายการที่เลือกไปด้วย
            FrmProductUpdel frmProductUpdel = new FrmProductUpdel(
               int.Parse(lvAllProduct.SelectedItems[0].SubItems[1].Text)
            );
            frmProductUpdel.ShowDialog();
            getAllProductToLv();
        }
    }
}
