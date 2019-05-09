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
    public partial class Form2 : Form
    {
        SqlDataAdapter ProductsAdapter = new SqlDataAdapter();
        SqlCommand WorkCommand = new SqlCommand();
        DataSet ProductsDataSet = new DataSet();
        int FlagSearch = 0;// флаг поиска: новый поиск - 0, далее - 1

        private Form1 SalesForm;

        public Form2(Form1 form1)
        {
            InitializeComponent();
            SalesForm = form1;
        }

        private void FillGrid(string Sql)//заполнение грида с товарами
        {
            ProductsDataSet.Clear();
            ProductsAdapter.SelectCommand = new SqlCommand(Sql, SalesForm.TestConnection);
            ProductsAdapter.SelectCommand.ExecuteScalar();
            ProductsAdapter.Fill(ProductsDataSet);
            dataGridView1.DataSource = ProductsDataSet.Tables[0];
            dataGridView1.Columns[0].Visible = false;
            if (ProductsDataSet.Tables[0].Rows.Count == 0)
            {
                textBox1.Enabled = false;
                SalesForm.ProductsData.Name="";
            }
            else
                textBox1.Enabled = true;
        }

        private int InsDelProduct(int Id, string Name, int FlagProc)//вызов хранимой процедуры добавление/удаление товаров
        {
            WorkCommand = SalesForm.TestConnection.CreateCommand();
            WorkCommand.CommandType = CommandType.StoredProcedure;
            WorkCommand.CommandText = "InsDelProduct";
            WorkCommand.Parameters.AddWithValue("@Id", Id);
            WorkCommand.Parameters.AddWithValue("@Name", Name);
            WorkCommand.Parameters.AddWithValue("@Flag", FlagProc);
            return WorkCommand.ExecuteNonQuery();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            FillGrid("select * from ProductsGrid()");
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)//выбор наименования товара
        {
            SalesForm.ProductsData.Name = dataGridView1.CurrentCell.Value.ToString();
            if (SalesForm.ProductsData.Name != "") SalesForm.ProductsData.Id = (int)dataGridView1.CurrentRow.Cells[0].Value;
            toolStripStatusLabel1.Text = "Товаров:" + ProductsDataSet.Tables[0].Rows.Count.ToString() + ". Выбран:" + SalesForm.ProductsData.Name;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)//изменение цвета текста в строке поиска
        {
            if (textBox1.Text.Length < 3)
                textBox1.ForeColor = Color.Red;
            else
                textBox1.ForeColor = Color.Green;
            FlagSearch = 0;
            button4.Text = "Поиск товара";
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)//начало поиска товара по Enter
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.button4_Click(null, null);
            }
        }

        private void button1_Click(object sender, EventArgs e) //добавление товара
        {
            if (textBox2.Text == "")
                MessageBox.Show("Не заполнено название товара.");
            else
            {
                int Answer = 0;
                if (textBox1.Enabled == false)
                    Answer = InsDelProduct(1, textBox2.Text, 0);
                else
                    Answer = InsDelProduct((int)ProductsDataSet.Tables[0].Rows[ProductsDataSet.Tables[0].Rows.Count - 1]["Id"] + 1, textBox2.Text, 0);
                if (Answer > 0)
                {
                    toolStripStatusLabel1.Text = "Товар " + textBox2.Text + " добавлен.";
                    textBox2.Clear();
                    FillGrid("select * from ProductsGrid()");
                }
                else
                    toolStripStatusLabel1.Text = "Такой товар уже есть в базе.";
            }
            WorkCommand.Dispose();
        }

        private void button2_Click(object sender, EventArgs e) // удаление товара
        {
            if (SalesForm.ProductsData.Name == "")
                MessageBox.Show("Не выбран товар для удаления.");
            else
            {
                if (InsDelProduct(SalesForm.ProductsData.Id, SalesForm.ProductsData.Name, 1) > 0)
                    FillGrid("select * from ProductsGrid()");
                else
                    toolStripStatusLabel1.Text = "Ошибка при удалении товара.";
            }
            WorkCommand.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)//выбор товара для продажи
        {
            if (ProductName == "")
                MessageBox.Show("Не выбран товар.");
            else
            {
                SalesForm.textBox1.Text = SalesForm.ProductsData.Name;
                this.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)//поиск товара
        {
            int i;
            if (FlagSearch == 0) i = 0; else i = dataGridView1.CurrentRow.Index + 1;
            FlagSearch = 0;
            while (FlagSearch != 1)
            {
                if (dataGridView1[1, i].Value.ToString().IndexOf(textBox1.Text) > -1)
                {
                    FlagSearch = 1;
                    button4.Text = "Искать далее";
                    dataGridView1.CurrentCell = dataGridView1[1, i];
                    if (i == dataGridView1.RowCount - 1)
                    {
                        button4.Text = "Поиск товара";
                        MessageBox.Show("Поиск закончен.");
                        break;
                    }
                }
                else
                    i++;
                if (i == dataGridView1.RowCount)
                {
                    button4.Text = "Поиск товара";
                    MessageBox.Show("Поиск закончен.");
                    break;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)//удаление из базы товаров без продаж
        {
            WorkCommand = SalesForm.TestConnection.CreateCommand();
            WorkCommand.CommandType = CommandType.StoredProcedure;
            WorkCommand.CommandText = "DeleteIdleProduct";
            int Answer = WorkCommand.ExecuteNonQuery();
            toolStripStatusLabel1.Text = "Товаров: " + ProductsDataSet.Tables[0].Rows.Count + ". Удалено товаров без продаж:" + Answer.ToString();
            FillGrid("select * from ProductsGrid()");
            WorkCommand.Dispose();
        }
    }
}
