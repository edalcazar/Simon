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

            // Suspend the game timer until the user starts the game
            gameTimer.Stop();

            // Critical assumption here is that all buttons must follow the 
            // naming pattern of "button[n]" where [n] is 1-4.
            button1.Click += handle_Click;
            button2.Click += handle_Click;
            button3.Click += handle_Click;
            button4.Click += handle_Click;

            refreshRecordLabels();
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startButton_Click(object sender, EventArgs e)
        {
            if (playLevel != 0)
            {
                // Stop the timer to give the user a chance to decide if they really want to 
                // start a new game. (Note this allows user to bypass timer restriction.)
                gameTimer.Stop();
                if (MessageBox.Show("Are you sure you want to abandon this game and start again?",
                    "Start New?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    gameTimer.Start();
                    return;
                }
            }
            initializeGame();
        }

        /// <summary>
        /// Handles click event for each tile (button) on the board
        /// </summary>
        /// <param name="sender">The button clicked</param>
        /// <param name="e"></param>
        private void handle_Click(object sender, EventArgs e)
        {
            Button tileClicked = (Button)sender;
            int buttonNumber = int.Parse(tileClicked.Name.Substring(6, 1));

            // If the right button was clicked, then...
            if (validClick(buttonNumber))
            {
                // Play the sound corresponding to the tile clicked
                playSound(buttonNumber);

                // Move to next sequence (and next level if necessary).
                currentSequence++;
                resumeNextSequence();
            }
        }

        /// <summary>
        /// Initializes a new game: 
        /// Populate sequence dictionary with the random sequence of numbers from 1-4.
        /// Initialize the playLevel and currentSequence variables.
        /// Call PlaySequence.
        /// Start GameTimer
        /// </summary>
        private void initializeGame()
        {
            Random randomNumber = new Random();
            sequence.Clear();
            for (currentSequence = 1; currentSequence <= maxLevel; currentSequence++)
            {
                sequence.Add(currentSequence, randomNumber.Next(1, 5));
            }
            playLevel = 0;
            currentSequence = 1;
            playSequence();
            gameTimer.Start();
        }

        /// <summary>
        /// "Hits" each of the tiles in the game sequence up to the currentSequence
        /// </summary>
        private void playSequence()
        {
            for (int i = 1; i <= currentSequence; i++)
            {
                Thread.Sleep(600);
                Button tileHit = (Button)Controls.Find("button" + sequence[i].ToString(), false)[0];
                tileHit.Hide();
                Thread.Sleep(10);
                playSound(sequence[i]);
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
        /// <returns>bool: true if correct button was clicked, false if not, or if no game is playing.</returns>
        private bool validClick(int buttonNumber)
        {
            if (currentSequence == 0) // Game has not started, so do nothing
                return false;

            // Stop the timer while we check the input (and play the next sequence if necessary)
            gameTimer.Stop();

            // If user clicked the wrong tile, then game over
            if (buttonNumber != sequence[currentSequence])
            {
                playSound(5);
                MessageBox.Show("Incorrect selection! You successfully completed " + (playLevel - 1) + " levels " +
                    "on this round.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkForRecordAndResetGame();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if user completed the entire round - if so, move to the next level
        /// </summary>
        private void resumeNextSequence()
        {
            // Just in case the user reaches the end...
            if (currentSequence > maxLevel)
            {
                MessageBox.Show("Congratulations! You have completed the final level (" + maxLevel + 
                    ")! You must be super-human!");
                checkForRecordAndResetGame();
                return;
            }

            if (currentSequence > playLevel)
            {
                // Round complete, move to next level.
                Thread.Sleep(500);
                playSequence();
                currentSequence = 1;
            }

            // Restart the timer - user has 7 seconds to make the next move
            gameTimer.Start(); 
        }

        /// <summary>
        /// Play the sound associated with the tile
        /// </summary>
        /// <param name="buttonNumber">The button number of the tile to play</param>
        private void playSound(int buttonNumber)
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
        private void checkForRecordAndResetGame()
        {
            if (playLevel - 1 > Records.RecordLevel)
            {
                string name = "NAME";
                if (showInputDialog(ref name) == DialogResult.OK)
                {
                    Records.SetNewRecord(name, playLevel - 1);
                    refreshRecordLabels();
                }
            }
            currentSequence = 0;
            playLevel = 0;
        }

        /// <summary>
        /// Updates the record holder labels on the form
        /// </summary>
        private void refreshRecordLabels()
        {
            recordLabel.Text = "Current Record: " + Records.RecordLevel;
            recordHolderLabel.Text = "Record Holder: " + 
                (string.IsNullOrEmpty(Records.RecordHolder) ? "None" : Records.RecordHolder);
        }

        /// <summary>
        /// The game timer allows a max of 7 seconds between tile clicks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            gameTimer.Stop();
            playSound(5);
            MessageBox.Show("Time's up! You successfully completed " + (playLevel - 1) + " levels " +
                "on this round.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Error);
            checkForRecordAndResetGame();
        }

        #region DialogHelper
        /// <summary>
        /// Helper dialog - mimics VB's InputBox without having to add reference to VB
        /// </summary>
        /// <param name="input">The name entered by user</param>
        /// <returns></returns>
        private static DialogResult showInputDialog(ref string input)
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
