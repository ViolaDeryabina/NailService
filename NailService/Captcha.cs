using System;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    public partial class Captcha : Form
    {
        private string captchaCode;
        private int attempts = 0;
        private const int MaxAttempts = 3;

        public Captcha()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            // Генерация случайного кода
            Random random = new Random();
            captchaCode = random.Next(1000, 9999).ToString();

            // Создание изображения с капчей
            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics graphics = Graphics.FromImage(bitmap);

            // Заливка фона
            graphics.Clear(Color.White);

            // Добавление шума (линии)
            Pen pen = new Pen(Color.LightGray);
            for (int i = 0; i < 10; i++)
            {
                int x1 = random.Next(bitmap.Width);
                int y1 = random.Next(bitmap.Height);
                int x2 = random.Next(bitmap.Width);
                int y2 = random.Next(bitmap.Height);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }

            // Рисование текста капчи
            Font font = new Font("Arial", 24, FontStyle.Bold | FontStyle.Italic);

            // Немного искажаем текст (случайное смещение букв)
            string captchaWithSpaces = string.Join(" ", captchaCode.ToCharArray());

            // Случайные цвета для букв
            Color[] colors = { Color.Black, Color.DarkBlue, Color.DarkGreen, Color.DarkRed };

            float x = 10;
            for (int i = 0; i < captchaWithSpaces.Length; i++)
            {
                Brush brush = new SolidBrush(colors[random.Next(colors.Length)]);
                float y = random.Next(5, 15); // Случайное смещение по вертикали
                graphics.DrawString(captchaWithSpaces[i].ToString(), font, brush, x, y);
                x += 20; // Расстояние между символами
            }

            // Добавление точек шума
            for (int i = 0; i < 50; i++)
            {
                int x1 = random.Next(bitmap.Width);
                int y1 = random.Next(bitmap.Height);
                bitmap.SetPixel(x1, y1, Color.LightGray);
            }

            pictureBox1.Image = bitmap;

            // Очистка поля ввода
            textBox1.Clear();
            labelAttempts.Text = $"Попыток осталось: {MaxAttempts - attempts}";
        }

        private void button1_Click(object sender, EventArgs e) // Проверить капчу
        {
            if (textBox1.Text == captchaCode)
            {
                MessageBox.Show("Капча введена верно!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Form1 show = new Form1();
                show.Show();
                this.Hide();
            }
            else
            {
                attempts++;

                if (attempts >= MaxAttempts)
                {
                    MessageBox.Show("Превышено количество попыток! Приложение будет закрыто.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show($"Неверный код! Осталось попыток: {MaxAttempts - attempts}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    GenerateCaptcha(); // Генерируем новую капчу
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) // Обновить капчу
        {
            GenerateCaptcha();
        }

        private void Captcha_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit(); // Закрываем приложение при попытке закрыть форму
            }
        }
    }
}