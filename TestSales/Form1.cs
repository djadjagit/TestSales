using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace TestSales
{
    public partial class Form1 : Form
    {
        Sales SalesData = new Sales();
        Sellers SellerData = new Sellers();
        public Products ProductsData = new Products();
        public SqlConnection TestConnection = new SqlConnection("Data Source=(localdb)\\devextreme;Initial Catalog=test_prommark;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        SqlDataAdapter SalesGridAdapter = new SqlDataAdapter();
        SqlDataAdapter WorkAdapter = new SqlDataAdapter();
        SqlCommand WorkCommand = new SqlCommand();
        DataSet SalesGridDataSet = new DataSet();
        DataSet WorkDataSet = new DataSet();
        int[] ArrayIdSeller;//массив Id продавцов для привязки к ComboBox Item Index

        public Form1()
        {
            InitializeComponent();
        }

        private void FillGrid(string Sql)//заполнение грида продаж
        {
            SalesGridDataSet.Clear();
            SalesGridAdapter.SelectCommand = new SqlCommand(Sql, TestConnection);
            SalesGridAdapter.SelectCommand.ExecuteScalar();
            SalesGridAdapter.Fill(SalesGridDataSet);
            dataGridView1.DataSource = SalesGridDataSet.Tables[0];
            if (SalesGridDataSet.Tables[0].Rows.Count == 0) dataGridView1.Enabled = false; else dataGridView1.Enabled = true;
            dataGridView1.Columns[5].Visible = false;
            dataGridView1.Columns[6].Visible = false;
        }

        private void FillCombo(string Sql)//заполнение комбо с продавцами
        {
            comboBox1.Items.Clear();
            comboBox1.Text = "";
            WorkAdapter.SelectCommand = new SqlCommand(Sql, TestConnection);
            WorkAdapter.SelectCommand.ExecuteScalar();
            WorkAdapter.Fill(WorkDataSet);
            if (WorkDataSet.Tables[0].Rows.Count==0)
                Array.Resize(ref ArrayIdSeller, 1);
            else
                Array.Resize(ref ArrayIdSeller, WorkDataSet.Tables[0].Rows.Count);
            ArrayIdSeller[0] = 0;
            for (int i = 0; i < WorkDataSet.Tables[0].Rows.Count; i++)
            {
                ArrayIdSeller[i] = (int)WorkDataSet.Tables[0].Rows[i]["Id"];
                comboBox1.Items.Add(WorkDataSet.Tables[0].Rows[i]["Name"]);
            }
            WorkDataSet.Reset();
        }

        private int UpInsDelSales(int Id, int ProductId, DateTime DateSale, decimal Price, int SellerId, int FlagProc)//вызов хранимой процедуры добавление/удаление товаров
        {
            WorkCommand = TestConnection.CreateCommand();
            WorkCommand.CommandType = CommandType.StoredProcedure;
            WorkCommand.CommandText = "UpInsDelSales";
            WorkCommand.Parameters.AddWithValue("@Id", Id);
            WorkCommand.Parameters.AddWithValue("@ProductId", ProductId);
            WorkCommand.Parameters.AddWithValue("@DateSale", DateSale);
            WorkCommand.Parameters.AddWithValue("@Price", Price);
            WorkCommand.Parameters.AddWithValue("@SellerId", SellerId);
            WorkCommand.Parameters.AddWithValue("@Flag", FlagProc);
            return WorkCommand.ExecuteNonQuery();
        }

        private int UpInsDelSeller(int Id, string Name, int FlagProc)//вызов хранимой процедуры добавление/изменение/удаление продавцов
        {
            WorkCommand = TestConnection.CreateCommand();
            WorkCommand.CommandType = CommandType.StoredProcedure;
            WorkCommand.CommandText = "UpInsDelSeller";
            WorkCommand.Parameters.AddWithValue("@Id", Id);
            WorkCommand.Parameters.AddWithValue("@Name", Name);
            WorkCommand.Parameters.AddWithValue("@Flag", FlagProc);
            return WorkCommand.ExecuteNonQuery();
        }

        private void ClearControls()//очистка рабочих контролов для ввода новых данных
        {
            textBox1.Clear();
            textBox2.Clear();
            comboBox1.Text = "";
            dateTimePicker1.Value = DateTime.Now;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
            try
            {
                TestConnection.Open();
                FillGrid("select * from SalesGrid()");
                FillCombo("select * from SellerCombo()");
                toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                this.Close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (button6.Text == "Изменить продавца")
            {
                SellerData.Id = ArrayIdSeller[comboBox1.SelectedIndex];
                SellerData.Name = comboBox1.Text;
            }
            else
                toolStripStatusLabel1.Text = "Включен режим редактирования продавца.";
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (button1.Text == "Редактировать запись")
            {
                if (dataGridView1.CurrentRow.Cells[0].Value.ToString() != "")
                {
                    SalesData.Id = (int)dataGridView1.CurrentRow.Cells[0].Value;
                    ProductsData.Name = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    SalesData.DateSale = (DateTime)dataGridView1.CurrentRow.Cells[2].Value;
                    SalesData.Price = (decimal)dataGridView1.CurrentRow.Cells[3].Value;
                    SellerData.Id = (int)dataGridView1.CurrentRow.Cells[5].Value;
                    ProductsData.Id = (int)dataGridView1.CurrentRow.Cells[6].Value;
                }
                else
                {
                    SalesData.Id = -1;
                    SellerData.Id = -1;
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)//контроль ввода цифр в поле сумма
        {
            try
            {
                Convert.ToDecimal(textBox2.Text);
            }
            catch
            {
                textBox2.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)//редактировать выделенную запись о продаже
        {
            if (button1.Text == "Редактировать запись")
            {
                if (SalesData.Id != -1)
                {
                    dataGridView1.Enabled = false;
                    button1.Text = "Отменить редактирвание";
                    button2.Enabled = false;
                    textBox1.Text = ProductsData.Name;
                    dateTimePicker1.Value = SalesData.DateSale;
                    textBox2.Text = SalesData.Price.ToString();
                    int i = 0;
                    while (ArrayIdSeller[i] != SellerData.Id) i++;
                    comboBox1.SelectedIndex = i;
                    toolStripStatusLabel1.Text = "Включен режим редактирования записи.";
                }
                else
                    MessageBox.Show("Не выбрана запись для редактирования.");
            }
            else
            {
                dataGridView1.Enabled = true;
                button1.Text = "Редактировать запись";
                button2.Enabled = true;
                toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count;
                ClearControls();
            }
        }

        private void button2_Click(object sender, EventArgs e)//удалить выделенную запись о продаже
        {
            if (SalesData.Id != 0)
            {
                if (UpInsDelSales(SalesData.Id, 0, dateTimePicker1.Value, 0, 0, 2) > 0)
                {
                    FillGrid("select * from SalesGrid()");
                    toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Запись о продаже удалена.";
                }
                else
                    toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Ошибка при удалении записи.";
            }
            WorkCommand.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)//открыть справочник товаров
        {
            Form2 ProductsForm = new Form2(this);
            ProductsForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)//добавить/изменить продавца
        {
            if (comboBox1.Text != "")
            {
                if (button6.Text == "Изменить продавца")
                    if (comboBox1.Items.Count == 0)
                        SellerData.Id = ArrayIdSeller[comboBox1.Items.Count] + 1;
                    else
                        SellerData.Id = ArrayIdSeller[comboBox1.Items.Count - 1] + 1;
                SellerData.Name = comboBox1.Text;
                if (UpInsDelSeller(SellerData.Id, SellerData.Name, 1) > 0)
                {
                    FillCombo("select * from SellerCombo()");
                    toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Продавец " + SellerData.Name + " сохранен.";
                    button6.Text = "Изменить продавца";
                    button7.Enabled = true;
                }
                else
                    toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Ошибка при сохранении продавца " + SellerData.Name;
                WorkCommand.Dispose();
            }
        }

        private void button5_Click(object sender, EventArgs e)//удалить продавца
        {
            if (comboBox1.Text != "")
            {
                if (UpInsDelSeller(SellerData.Id, "", 2) > 0)
                {
                    FillCombo("select * from SellerCombo()");
                    toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Продавец " + SellerData.Name + " удален из базы.";
                    SellerData.Id = 0;
                }
                else
                    toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Ошибка при удалении продавца " + SellerData.Name;
                WorkCommand.Dispose();
            }
        }

        private void button6_Click(object sender, EventArgs e)//включение режима изменения продавца
        {
            if (SellerData.Id != -1)
            {
                if (button6.Text == "Изменить продавца")
                {
                    button6.Text = "Отменить изменения";
                    button7.Enabled = false;
                }
                else
                {
                    button6.Text = "Изменить продавца";
                    button7.Enabled = true;
                }
            }
            else
                MessageBox.Show("Выбери продавца для изменения");
        }

        private void button7_Click(object sender, EventArgs e)//главная кнопка сохранения записи о продаже
        {
            if ((textBox1.Text == "") || (textBox2.Text == "") || (comboBox1.Text == ""))
                MessageBox.Show("Проверь заполнение обязательных данных.");
            else
            {
                if (SellerData.Id > -1)
                {
                    if (button1.Text == "Редактировать запись")
                        if (dataGridView1.Enabled==false)
                            SalesData.Id = 1;
                        else
                            SalesData.Id = (int)SalesGridDataSet.Tables[0].Rows[SalesGridDataSet.Tables[0].Rows.Count - 1]["N"] + 1;
                    if (UpInsDelSales(SalesData.Id, ProductsData.Id, dateTimePicker1.Value, Convert.ToDecimal(textBox2.Text), SellerData.Id, 1) > 0)
                    {
                        FillGrid("select * from SalesGrid()");
                        toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Запись о продаже сохранена.";
                        button1.Text = "Редактировать запись";
                        button2.Enabled = true;
                        ClearControls();
                    }
                    else
                        toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Ошибка при сохранении записи.";
                    WorkCommand.Dispose();
                }
                else
                    MessageBox.Show("В базе нет такого продавца.");
            }
        }

        private void button8_Click(object sender, EventArgs e)//удаление продавцов из базы, у которых нет продаж
        {
            WorkCommand = TestConnection.CreateCommand();
            WorkCommand.CommandType = CommandType.StoredProcedure;
            WorkCommand.CommandText = "DeleteIdleSeller";
            int Answer = WorkCommand.ExecuteNonQuery();
            toolStripStatusLabel1.Text = "Продаж: " + SalesGridDataSet.Tables[0].Rows.Count + ". Удалено продавцов без продаж:" + Answer.ToString();
            FillCombo("select * from SellerCombo()");
            WorkCommand.Dispose();
        }
    }
}
