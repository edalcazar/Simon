using System;
using System.Collections.Generic;
using System.Threading;
using System.Media;
using System.Windows.Forms;
using System.IO;

namespace Edmon
{
    public partial class Edmon : Form
    {
        Dictionary<int, int> sequence = new Dictionary<int, int>();
        int currentSequence;
        int playLevel;
        const int maxLevel = 100;

        public Edmon()
        {
            InitializeComponent();

            // Critical assumption here is that all buttons must follow the 
            // naming pattern of "button[n]" where [n] is 1-4.
            button1.Click += Handle_Click;
            button2.Click += Handle_Click;
            button3.Click += Handle_Click;
            button4.Click += Handle_Click;

            RefreshRecordLabels();
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (playLevel != 0)
            {
                if (MessageBox.Show("Are you sure you want to abandon this game and start again?",
                    "Start New?", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            InitializeGame();
        }

        /// <summary>
        /// Handles click event for each tile (button) on the board
        /// </summary>
        /// <param name="sender">The button clicked</param>
        /// <param name="e"></param>
        private void Handle_Click(object sender, EventArgs e)
        {
            Button tileClicked = (Button)sender;
            string buttonNumber = tileClicked.Name.Substring(6, 1);
            CheckSequence(int.Parse(buttonNumber));
        }

        /// <summary>
        /// Initializes a new game: 
        /// Populate sequence dictionary with the random sequence of numbers from 1-4.
        /// Initialize the playLevel and currentSequence variables.
        /// Call PlaySequence.
        /// </summary>
        private void InitializeGame()
        {
            Random randomNumber = new Random();
            sequence.Clear();
            for (currentSequence = 1; currentSequence <= maxLevel; currentSequence++)
            {
                sequence.Add(currentSequence, randomNumber.Next(1, 5));
            }
            playLevel = 0;
            currentSequence = 1;
            PlaySequence();
        }

        /// <summary>
        /// "Hits" each of the tiles in the game sequence up to the currentSequence
        /// </summary>
        private void PlaySequence()
        {
            for (int i = 1; i <= currentSequence; i++)
            {
                Thread.Sleep(600);
                Button tileHit = (Button)Controls.Find("button" + sequence[i].ToString(), false)[0];
                tileHit.Hide();
                Thread.Sleep(10);
                PlaySound(sequence[i]);
                Thread.Sleep(10);
                tileHit.Show();
                Refresh();
            }
            playLevel++;
        }

        /// <summary>
        /// Check if the tile clicked corresponds to the currentSequence value
        /// </summary>
        /// <param name="buttonNumber">The button number of the tile that was clicked.</param>
        private void CheckSequence(int buttonNumber)
        {
            if (currentSequence == 0) // Game has not started, so do nothing
                return;

            // User clicked the wrong tile, game over
            if (buttonNumber != sequence[currentSequence])
            {
                PlaySound(5);
                MessageBox.Show("Incorrect selection! You successfully completed " + (playLevel - 1) + " levels " +
                    "on this round.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CheckForRecordAndReInitialize();
                return;
            }

            // Play the sound corresponding to the tile clicked
            PlaySound(buttonNumber);

            currentSequence++;
            if (currentSequence > maxLevel)
            {
                MessageBox.Show("Congratulations! You have completed the final level (" + maxLevel + 
                    ")! You must be super-human!");
                CheckForRecordAndReInitialize();
                return;
            }

            // Check if user completed the entire round - if so, move to the next level
            if (currentSequence > playLevel)
            {
                Thread.Sleep(500);
                PlaySequence();
                currentSequence = 1;
            }

            // TODO: We could introduce a timer element to limit the user's response time
            // based on the currentSequence level.
            // i.e., to help prevent cheating by means of recording the sequence.
        }

        /// <summary>
        /// Play the sound associated with the tile
        /// </summary>
        /// <param name="buttonNumber">The button number of the tile to play</param>
        private void PlaySound(int buttonNumber)
        {
            Stream soundStream = null;
            switch (buttonNumber)
            {
                case 1:
                    soundStream = Properties.Resources.one; break;
                case 2:
                    soundStream = Properties.Resources.two; break;
                case 3:
                    soundStream = Properties.Resources.three; break;
                case 4:
                    soundStream = Properties.Resources.four; break;
                case 5: // The "game over" sound
                    soundStream = Properties.Resources.glass_shatter_c; break;
            }
            SoundPlayer player = new SoundPlayer(soundStream);
            player.PlaySync();
        }

        /// <summary>
        /// At the end of each game, check if a new record was set (sets the new record info if necessary),
        /// and re-initialize currentSequence and playLevel so user can't replay the same game sequence.
        /// </summary>
        private void CheckForRecordAndReInitialize()
        {
            if (playLevel - 1 > Records.RecordLevel())
            {
                string name = "NAME";
                if (ShowInputDialog(ref name) == DialogResult.OK)
                {
                    Records.SetNewRecord(name, playLevel - 1);
                    RefreshRecordLabels();
                }
            }
            currentSequence = 0;
            playLevel = 0;
        }

        private void RefreshRecordLabels()
        {
            RecordLabel.Text = "Current Record: " + Records.RecordLevel();
            RecordHolderLabel.Text = "Record Holder: " + 
                (string.IsNullOrEmpty(Records.RecordHolder()) ? "None" : Records.RecordHolder());
        }

        #region DialogHelper
        /// <summary>
        /// Helper dialog - mimics VB's InputBox without having to add reference to VB
        /// </summary>
        /// <param name="input">The name entered by user</param>
        /// <returns></returns>
        private static DialogResult ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 150);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Name";

            Label label = new Label();
            label.Size = new System.Drawing.Size(size.Width - 10, 30);
            label.Location = new System.Drawing.Point(10, 20);
            label.Text = "Congratulations, you set a new record. Please enter your name below.";
            inputBox.Controls.Add(label);

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 65);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 99);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 99);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }
        #endregion
    }
}
