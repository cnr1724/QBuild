using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Qbuild
{
    public partial class Form2 : Form
    {
        private string connectionString = "Data Source=LAPTOP-MSRLEJ67;Initial Catalog=QbuildDB;Integrated Security=True";

        public Form2()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedPartName = e.Node.Text;
            dataGridView1.Rows.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string selectComponentPartsQuery = "SELECT * FROM tblBOM WHERE ParentName = @ParentName";
                using (SqlCommand selectComponentPartsCommand = new SqlCommand(selectComponentPartsQuery, connection))
                {
                    selectComponentPartsCommand.Parameters.AddWithValue("@ParentName", selectedPartName);
                    using (SqlDataReader reader = selectComponentPartsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string componentName = reader["ComponentName"].ToString();
                            decimal quantity = Convert.ToDecimal(reader["Quantity"]);

                            using (SqlConnection partInfoConnection = new SqlConnection(connectionString))
                            {
                                partInfoConnection.Open();

                                string selectPartInfoQuery = "SELECT * FROM tblParts WHERE Name = @Name";
                                using (SqlCommand selectPartInfoCommand = new SqlCommand(selectPartInfoQuery, partInfoConnection))
                                {
                                    selectPartInfoCommand.Parameters.AddWithValue("@Name", componentName);
                                    using (SqlDataReader partInfoReader = selectPartInfoCommand.ExecuteReader())
                                    {
                                        if (partInfoReader.Read())
                                        {
                                            string partNumber = partInfoReader["PartNumber"].ToString();
                                            string title = partInfoReader["Title"].ToString();
                                            string type = partInfoReader["Type"].ToString();
                                            string item = partInfoReader["Item"].ToString();
                                            string material = partInfoReader["Material"].ToString();

                                            dataGridView1.Rows.Add(                                          
                                                partNumber,
                                                title,                                          
                                                type,
                                                item,
                                                material
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    

        private void BuildSubtree(TreeNode parentNode, string partName, int level)
        {
            if (level > 5)
                return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string selectChildPartsQuery = "SELECT ComponentName FROM tblBOM WHERE ParentName = @ParentName";
                using (SqlCommand selectChildPartsCommand = new SqlCommand(selectChildPartsQuery, connection))
                {
                    selectChildPartsCommand.Parameters.AddWithValue("@ParentName", partName);
                    using (SqlDataReader reader = selectChildPartsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string childPartName = reader["ComponentName"].ToString();
                            var childNode = new TreeNode(childPartName);
                            parentNode.Nodes.Add(childNode);
                            BuildSubtree(childNode, childPartName, level + 1);
                        }
                    }
                }
            }
        }

        private void BuildTreeView()
        {
            treeView1.Nodes.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string selectRootPartsQuery = "SELECT DISTINCT ComponentName FROM tblBOM WHERE ParentName IS NULL";
                using (SqlCommand selectRootPartsCommand = new SqlCommand(selectRootPartsQuery, connection))
                using (SqlDataReader reader = selectRootPartsCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string rootPartName = reader["ComponentName"].ToString();
                        var rootTreeNode = new TreeNode(rootPartName);
                        BuildSubtree(rootTreeNode, rootPartName, 1);
                        treeView1.Nodes.Add(rootTreeNode);
                    }
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("ComponentName", "ComponentName");
            dataGridView1.Columns.Add("PartNumber", "PartNumber");
            dataGridView1.Columns.Add("Title", "Title");
            dataGridView1.Columns.Add("Quantity", "Quantity");
            dataGridView1.Columns.Add("Type", "Type");
            dataGridView1.Columns.Add("Item", "Item");
            dataGridView1.Columns.Add("Material", "Material");

            BuildTreeView();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }


}
