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
using System.Text.RegularExpressions;

namespace Proiect_florea
{
    public partial class Form1 : Form
    {

        bool InputCorect = true;
        bool uniport = false;
        bool fileChosen = false;

        string[] fetchRateVals = new string[] { "4", "8", "16" };
        string[] irMaxVals = new string[] { "2", "4", "8", "16" };
        string[] ibsVals = new string[] { "4", "8", "16", "32" };
        string[] nPenVals = new string[] { "10", "15", "20" };
        string[] registerVals = new string[] { "2", "4", "8", "16" };
        string[] blockSizeICVals = new string[] { "4", "8", "16" };
        string[] blockSizeDCVals = new string[] { "4", "8", "16" };
        string[] sizeICDCVals = new string[] { "64", "128", "256", "512", "1024", "2048", "4096", "8192" };

        List<Instructiune> instructionsFromBenchmark = new List<Instructiune>();
        Instructiune[,] instructionsFromMemory;

        int loadInstructions = 0;
        int storeInstructions = 0;
        int branchInstructions = 0;
        int arithmeticInstructions = 0;
        int totalInstructions = 0;

        int availableAccessToMemoryPerCycle = 0;
        int numberOfMemoryAccesses = 0;
        int missCachePenalty = 0;
        double cacheMiss = 0.1;

        int oneCycle = 0;
        int ticks = 0;
        double issueRate = 0;

        int IRmax;
        int rowsNumber = 0;
        int PCnormal;

        string filePath = string.Empty;
        List<string> fileContent;

        public Form1()
        {
            InitializeComponent();
            
        }
       

        private void Form1_Load(object sender, EventArgs e)
        {           

            cmbBoxFR.Items.AddRange(fetchRateVals);
            cmbBoxIrMax.Items.AddRange(irMaxVals);
            cmbIBX.Items.AddRange(ibsVals);
            cmbN_Pen.Items.AddRange(nPenVals);
            cmbNSR.Items.AddRange(registerVals);
            cmbBSize.Items.AddRange(blockSizeICVals);
            cmbSize_IC.Items.AddRange(blockSizeDCVals);
            cmbDCBSize.Items.AddRange(sizeICDCVals);
            cmbDCSize_IC.Items.AddRange(sizeICDCVals);

            cmbBSize.SelectedIndex = 0;
            cmbSize_IC.SelectedIndex = 0;
            cmbDCBSize.SelectedIndex = 0;
            cmbDCSize_IC.SelectedIndex = 0;
        }

        private void WriteLoadInstructions()
        {
            loadText.Text = Convert.ToString(loadInstructions);
        }
        private void WriteStoreInstructions()
        {
            storeText.Text = Convert.ToString(storeInstructions);
        }
        private void WriteBranchInstructions()
        {
            branchText.Text = Convert.ToString(branchInstructions);
        }
        private void WriteTotalInstructions()
        {
            totalText.Text = Convert.ToString(totalInstructions);
        }

        private void WriteOneCycle()
        {
            oneCycleText.Text = Convert.ToString(oneCycle);
         }

        private void WriteIssueRate()
        {
            issueRateText.Text = Convert.ToString(issueRate);
        }
        private void WriteTicks()
        {
            ticksText.Text = Convert.ToString(ticks);
        }


        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select A File";
            openDialog.Filter = "Trace (*.trc)|*.trc" + "|" +
                                "All Files (*.*)|*.*";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openDialog.FileName;
                fileChosen = true;
            }

            ParseFile();

        }

        private void ParseFile()
        {
            instructionsFromBenchmark.Clear();

            if (!String.IsNullOrEmpty(filePath))
            {
                fileContent = File.ReadAllText(filePath).Split(' ').Select(a => a.Trim()).ToList();
                fileContent = fileContent.Where(x => string.IsNullOrWhiteSpace(x) == false).ToList();
                for (int i = 0; i < fileContent.Count; i += 3)
                {
                    Instructiune instruction = new Instructiune
                    {
                        instructionType = GetInstructionType(fileContent[i]),
                        currentPC = Convert.ToInt32(fileContent[i + 1]),
                        targetAddress = Convert.ToInt32(fileContent[i + 2])
                    };
                    instructionsFromBenchmark.Add(instruction);
                }
            }
        }

        private InstructionType? GetInstructionType(string instructionShortcut)
        {
            switch (instructionShortcut)
            {
                case "B":
                    return InstructionType.Branch;
                case "L":
                    return InstructionType.Load;
                case "S":
                    return InstructionType.Store;
                default: break;
            }
            return null;
        }

        private void ResetAll()
        {
            loadInstructions = 0;
            storeInstructions = 0;
            branchInstructions = 0;
            arithmeticInstructions = 0;
            totalInstructions = 0;
        }

        private void btnSim_MouseClick(object sender, MouseEventArgs e)
        {

            if(radioButton1.Checked)
                loadText.BackColor = Color.Red;
            else 
                loadText.BackColor = Color.Green;

            if (fileChosen)
            {
                ResetAll();

                PCnormal = 0;

                instructionsFromMemory = new Instructiune[100000000, IRmax];

                ExecuteInstructions();
                CalculateStatistics();

                WriteLoadInstructions();
                WriteStoreInstructions();
                WriteBranchInstructions();

                WriteIssueRate();
                WriteTicks();

                WriteTotalInstructions();

                //DeactivateButtons();

                instructionsFromMemory = null;
            }

                Rezultat form = new Rezultat();
            form.Show();
        }

        private void ExecuteInstructions()
        {
            int row = 0;
            int col = 0;
            try
            {
                foreach (Instructiune instruction in instructionsFromBenchmark)
                {
                    while (instruction.currentPC != PCnormal)
                    {
                        if (col == IRmax)
                        {
                            row++;
                            col = 0;
                        }

                        //adauga o instructiune alu in matricea instructiuniAduseDinMemorie
                        instructionsFromMemory[row, col++] = new Instructiune
                        {
                            instructionType = InstructionType.Arithmetic
                        };

                        PCnormal++;
                        arithmeticInstructions++;
                    }

                    if (instruction.instructionType == InstructionType.Branch)
                    {
                        if (col == IRmax)
                        {
                            row++;
                            col = 0;
                        }

                        //adauga o instructiune B in matricea instructiuniAduseDinMemorie
                        instructionsFromMemory[row, col++] = instruction;

                        PCnormal = instruction.targetAddress;
                        branchInstructions++;
                    }

                    if (instruction.instructionType == InstructionType.Store)
                    {
                        if (col == IRmax)
                        {
                            row++;
                            col = 0;
                        }

                        //adauga o instructiune S in matricea instructiuniAduseDinMemorie
                        instructionsFromMemory[row, col++] = instruction;

                        PCnormal++;
                        storeInstructions++;
                    }

                    if (instruction.instructionType == InstructionType.Load)
                    {
                        if (col == IRmax)
                        {
                            row++;
                            col = 0;
                        }

                        //adauga o instructiune L in matricea instructiuniAduseDinMemorie
                        instructionsFromMemory[row, col++] = instruction;

                        PCnormal++;
                        loadInstructions++;
                    }
                }

                rowsNumber = row;
            }
            catch (Exception ex)
            {

            }
        }

        private void CalculateStatistics()
        {
            var latency = Convert.ToInt32(latenta.Value);
            var penalties = Convert.ToInt32(cmbN_Pen.SelectedItem);

            ticks = latency * rowsNumber;

            missCachePenalty = Convert.ToInt32(loadInstructions * cacheMiss * missCachePenalty);
            ticks += missCachePenalty;

            foreach (Instructiune instruction in instructionsFromMemory)
            {
                if (instruction != null)
                {
                    if (numberOfMemoryAccesses < availableAccessToMemoryPerCycle)
                    {
                        if (instruction.instructionType == InstructionType.Load || instruction.instructionType == InstructionType.Store)
                        {
                            numberOfMemoryAccesses++;
                        }
                    }
                    else
                    {
                        if (instruction.instructionType == InstructionType.Load || instruction.instructionType == InstructionType.Store)
                        {
                            numberOfMemoryAccesses = 0;
                            ticks += latency;
                        }
                    }
                }
            }

            totalInstructions = loadInstructions + storeInstructions + branchInstructions + arithmeticInstructions;
            issueRate = (Convert.ToDouble(totalInstructions) / Convert.ToDouble(ticks));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmbBoxFR_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var valueFR = Convert.ToInt32(cmbBoxFR.SelectedItem);
            var valueIBS = Convert.ToInt32(cmbIBX.SelectedItem);
            var valueIR = Convert.ToInt32(cmbBoxIrMax.SelectedItem);

            if (valueIBS != 0) //valoare default 0 daca nu e setat nimic
            {
                if (!(valueIBS >= valueFR)) //IBS >= FR
                {
                    MessageBox.Show("Alegeti alta valoare pentru FR; a se tine cont ca FR <= IBS.", "Fetch Rate incorect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Input.ConditiaDoi = false;
                }
                else
                {
                    Input.ConditiaDoi = true;
                }
            }


            if (valueIR != 0) //valoare default 0 daca nu e setat nimic
            {
                if (!(valueIR <= valueFR)) //IR <= FR
                {
                    MessageBox.Show("Alegeti alta valoare pentru FR; a se tine cont ca IR <= FR.", "Fetch Rate incorect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Input.ConditiaTrei = false;
                }
                else
                {
                    Input.ConditiaTrei = true;
                }
            }
        }

        private void cmbIBX_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var valueFR = Convert.ToInt32(cmbBoxFR.SelectedItem);
            var valueIBS = Convert.ToInt32(cmbIBX.SelectedItem);
            var valueIR = Convert.ToInt32(cmbBoxIrMax.SelectedItem);

            if (valueFR != 0) //valoare default 0 daca nu e setat nimic
            {
                if (!(valueIBS >= valueFR)) //IBS >= FR
                {
                    MessageBox.Show("Alegeti alta valoare pentru IR; a se tine cont ca IBS >= FR.", "Issue Rate maxim incorect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Input.ConditiaDoi = false;
                }
                else
                {
                    Input.ConditiaDoi = true;
                }

                if (valueIR != 0) //valoare default 0 daca nu e setat nimic
                {
                    if (!(valueIBS >= valueFR + valueIR)) //IBS >= FR + IR
                    {
                        MessageBox.Show("Alegeti alta valoare pentru IR; a se tine cont ca IBS >= FR + IR.", "Issue Rate maxim incorect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Input.ConditiaUnu = false;
                    }
                    else
                    {
                        Input.ConditiaUnu = true;
                    }
                }
            }
        }


        private void cmbBoxIrMax_SelectionChangeCommitted_1(object sender, EventArgs e)
        {
            var valueFR = Convert.ToInt32(cmbBoxFR.SelectedItem);
            var valueIR = Convert.ToInt32(cmbBoxIrMax.SelectedItem);
            var valueIBS = Convert.ToInt32(cmbIBX.SelectedItem);

            IRmax = valueIR;

            if (valueFR != 0) //valoare default 0 daca nu e setat nimic
            {
                if (!(valueIR <= valueFR)) //IR <= FR
                {
                    MessageBox.Show("Alegeti alta valoare pentru IR; a se tine cont ca IR <= FR.", "Issue Rate maxim incorect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Input.ConditiaTrei = false;
                }
                else
                {
                    Input.ConditiaTrei = true;
                }

                if (valueIBS != 0) //valoare default 0 daca nu e setat nimic
                {
                    if (!(valueIBS >= valueIR)) //IBS >= FR
                    {
                        MessageBox.Show("Alegeti alta valoare pentru IR; a se tine cont ca IBS >= FR.", "Issue Rate maxim incorect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Input.ConditiaDoi = false;
                    }
                    else
                    {
                        Input.ConditiaDoi = true;
                    }
                }
            }
        }

        private void cmbBoxFR_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbBoxFR.Items.Contains(cmbBoxFR.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.FRcorect = false;
            }
            else
            {
                Input.FRcorect = true;
            }
        }

        private void cmbBoxIrMax_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbBoxIrMax.Items.Contains(cmbBoxIrMax.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.IRmaxcorect = false;
            }
            else
            {
                Input.IRmaxcorect = true;
            }
        }

        private void cmbIBX_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbIBX.Items.Contains(cmbIBX.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.IBScorect = false;
            }
            else
            {
                Input.IBScorect = true;
            }
        }

        private void cmbN_Pen_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbN_Pen.Items.Contains(cmbN_Pen.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.MissCachecorect = false;
            }
            else
            {
                Input.MissCachecorect = true;
            }
        }

        private void cmbNSR_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbNSR.Items.Contains(cmbNSR.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.RegSetNumbercorect = false;
            }
            else
            {
                Input.RegSetNumbercorect = true;
            }
        }

        private void cmbBSize_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbBSize.Items.Contains(cmbBSize.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.BlockSizeInstrcorect = false;
            }
            else
            {
                Input.BlockSizeInstrcorect = true;
            }
        }

        private void cmbSize_IC_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbSize_IC.Items.Contains(cmbSize_IC.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.SizeICcorect = false;
            }
            else
            {
                Input.SizeICcorect = true;
            }
        }

        private void cmbDCBSize_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbDCBSize.Items.Contains(cmbDCBSize.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.BlockSizeDatacorect = false;
            }
            else
            {
                Input.BlockSizeDatacorect = true;
            }
        }

        private void cmbDCSize_IC_TextChanged_1(object sender, EventArgs e)
        {
            if (!cmbDCSize_IC.Items.Contains(cmbDCSize_IC.Text))
            {
                MessageBox.Show("Alegeti o valoare din dropdown!", "Valoare invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Input.SizeDCcorect = false;
            }
            else
            {
                Input.SizeDCcorect = true;
            }
        }
    }
}
