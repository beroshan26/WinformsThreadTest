using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winformThreadTest
{
    public partial class Form1 : Form
    {
        WindowsFormsSynchronizationContext context;
        delegate void updatedelegate();

        public Form1()
        {
            InitializeComponent();
            context = ((WindowsFormsSynchronizationContext)SynchronizationContext.Current);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread.Sleep(5000);
            textBox1.Text = "Slept for 5 seconds";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread t1 = new Thread(sleepAsync);
            t1.Start();            
        }

        private void sleepAsync()
        {
            Thread.Sleep(5000);
            //textBox2.Text = "Slept async for 10 sec"; //errors on cross thread operation
                       
            context.Post(update, null); //using windows forms sync context to avoid cross thread operation

            //Action action = () => textBox2.Text = "Slept async for 5 sec";
            //this.Invoke(action);

            //updatedelegate updDel = new updatedelegate(updateText);
            //this.Invoke(updDel);
        }

        private void update(object data)
        {
            textBox2.Text = "Slept async for 5 sec";
        }

        private void updateText()
        {
            textBox2.Text = "Slept async for 5 sec";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var list = new List<string>();

            Parallel.Invoke(
            () => list.Add(new WebClient().DownloadString("http://www.google.com")),
            () => list.Add(new WebClient().DownloadString("http://www.dell.com")),
            () => list.Add(new WebClient().DownloadString("http://www.linqpad.net")));

            Parallel.For(0, 5, m => Console.Write(m));
           
            MessageBox.Show("done");
        }

        TaskScheduler uiSched;
        private void button4_Click(object sender, EventArgs e)
        {
            //var tst = Task.Factory.StartNew<string>(taskFunc);
            //MessageBox.Show(tst.Result);

            Task.Factory.StartNew<string>(taskFunc).ContinueWith(a => MessageBox.Show(a.Result));
        }

        private string taskFunc()
        {
            Thread.Sleep(4000);
            return new WebClient().DownloadString("http://www.google.com");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int x = 0;
            
            try
            {
                Task<int> task = Task.Factory.StartNew(() => 7 / x);
                MessageBox.Show(task.Result.ToString());
            }
            catch (AggregateException aex)
            {
                MessageBox.Show(aex.InnerException.Message);
            }
        }

        private void cancelFunc()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!cancelSrc.IsCancellationRequested)
                {
                    Thread.Sleep(2000);
                    MessageBox.Show("performing background work");
                }
            }
            //token.ThrowIfCancellationRequested();
            MessageBox.Show("Done without cancelled");
        }

        static CancellationTokenSource cancelSrc = new CancellationTokenSource();
        static CancellationToken token = cancelSrc.Token;

        private void button6_Click(object sender, EventArgs e)
        {            
            Task task = Task.Factory.StartNew(cancelFunc, token);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            cancelSrc.Cancel();
            MessageBox.Show("cancelled");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            calldataAsync();
            label1.Text = "working...";
        }

        private async void CallLongrunningmethod()
        {
            string result = await LongRunningMethodAsync("World");
            label1.Text = result;
        }

        private Task<string> LongRunningMethodAsync(string message)
        {
            return Task.Run<string>(() => LongRunningMethod(message));
        }

        private string LongRunningMethod(string message)
        {
            Thread.Sleep(5000);
            return "Hello" + message;
        }

        private string getwebdata()
        {
            Thread.Sleep(4000);
            return new WebClient().DownloadString("http://www.google.com");
        }

        private async void calldataAsync()
        {
            string result = await getwebdataAsync();
            label1.Text = "Done";
        }

        private Task<string> getwebdataAsync()
        {            
            return Task.Run(() => getstringdata());            
        }

        private string getstringdata()
        {
            Thread.Sleep(3000);
            return new WebClient().DownloadString("http://www.google.com");
        }

        AutoResetEvent ar = new AutoResetEvent(false);
        private void button8_Click(object sender, EventArgs e)
        {
            Thread tar = new Thread(waiter);
            tar.Start();
            ar.Set();
        }

        private void waiter()
        {
            ar.WaitOne();
            Thread.Sleep(2000);
          Thread tmr1 = new Thread(mrwaiter1);
            tmr1.Start();

            Thread tmr2 = new Thread(mrwaiter2);
            tmr2.Start();

            Thread tmr3 = new Thread(mrwaiter3);
            tmr3.Start();

            Thread.Sleep(3000);
            mr.Set();  context.Post(update, null); //using windows forms sync context to avoid cross thread operation

        }

        ManualResetEvent mr = new ManualResetEvent(false);
        private void button9_Click(object sender, EventArgs e)
        {
            
        }

        private void mrwaiter1()
        {
            mr.WaitOne();
            //Thread.Sleep(2000);
            context.Post(updatetext, "1"); 
        }

        private void mrwaiter2()
        {
            mr.WaitOne();
            //Thread.Sleep(2000);
            context.Post(updatetext, "2"); 
        }

        private void mrwaiter3()
        {
            mr.WaitOne();
            //Thread.Sleep(2000);
            context.Post(updatetext, "3"); 
        }

        private void updatetext(object data)
        {
            string str = ((string)data);
            textBox3.Text += "manual signal " + str + Environment.NewLine;
        }

        CountdownEvent ce = new CountdownEvent(3);
        private void button10_Click(object sender, EventArgs e)
        {
            Thread tmr1 = new Thread(cewaiter1);
            tmr1.Start();

            Thread tmr2 = new Thread(cewaiter2);
            tmr2.Start();

            Thread tmr3 = new Thread(cewaiter3);
            tmr3.Start();

            ce.Wait();
            context.Post(updatetext, "ce");
        }

        private void cewaiter1()
        {            
            Thread.Sleep(2000);
            ce.Signal();
        }
        private void cewaiter2()
        {
            Thread.Sleep(2000);
            ce.Signal();
        }

        private void cewaiter3()
        {            
            Thread.Sleep(2000);
            ce.Signal();
        }

        private async void button11_Click(object sender, EventArgs e)
        {
            Observable.Start(() =>
            {
                Thread.Sleep(2000);
                MessageBox.Show("Rx");
            });

            var o = Observable.Start(() =>
            {
                //This starts on a background thread.
                MessageBox.Show("From background thread. Does not block main thread.");
                MessageBox.Show("Calculating...");
                Thread.Sleep(5000);
                MessageBox.Show("Background work completed.");
            });

            await o.FirstAsync();   // subscribe and wait for completion of background operation. 
            //If you remove await, the main thread will complete first.
            MessageBox.Show("Main thread completed.");
        }

        ObservableCollection<Person> coll;
        private BindingList<Person> personcoll;
        private void button12_Click(object sender, EventArgs e)
        {
            coll = new ObservableCollection<Person>()
            {
                new Person () { Name = "Roshan", Age = 33},
                new Person(){ Name = "test", Age=22}
            };

            personcoll = new BindingList<Person>
            {
                new Person() {Name = "test", Age = 22},
                new Person() {Name = "Roshan", Age = 33}
            };

            //dataGridView1.DataSource = new BindingSource(){DataSource = coll};
            dataGridView1.DataSource = personcoll;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            coll[0].Name = "Roshan123";
            coll.Add(new Person() { Name = "Rosha1n", Age = 33 });
            coll.Add(new Person() { Name = "test1", Age = 22 });

            personcoll[0].Name = "Roshan123";
            personcoll.Add(new Person() { Name = "Rosha1n", Age = 33 });
            personcoll.Add(new Person() { Name = "test1", Age = 22 });
        }

        private void button14_Click(object sender, EventArgs e)
        {
            uiSched = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew<string>(taskFunc)
                .ContinueWith(a => MessageBox.Show(a.Result), uiSched);
        }

        AutoResetEvent arev = new AutoResetEvent(false);
        private void button16_Click(object sender, EventArgs e)
        {
            Thread tar1 = new Thread(arwaiter1);
            tar1.Start();

            Thread tar2 = new Thread(arwaiter2);
            tar2.Start();

            Thread.Sleep(3000);
            arev.Set();
        }

        private void arwaiter1()
        {
            arev.WaitOne();
            
            context.Post(updatetext, "ar1");
        }

        private void arwaiter2()
        {
            arev.WaitOne();

            context.Post(updatetext, "ar2");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            TaskCreationOptions atp = TaskCreationOptions.AttachedToParent;

            Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() => { throw null; }, atp);
                Task.Factory.StartNew(() => { throw null; }, atp);
                Task.Factory.StartNew(() => { throw null; }, atp);
            }).ContinueWith(p => MessageBox.Show(p.Exception.Message), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            var res = Task<string>.Run(() => getstringdata());
            MessageBox.Show(res.Result);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            var contents = new List<Func<int>>();
            var s = new StringBuilder();
            for (var i = 4; i < 7; i++)
            {
                int j = i;
                contents.Add(()=> i);
            }

            foreach (Func<int> t in contents)
            {
                s.Append(t());
            }
            MessageBox.Show(s.ToString());
        }
    }

    public class Person : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get
            {
                return name; 
            }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
            
        }
        private int age;
        public int Age
        {
            get
            {
                return age;
            }
            set
            {
                age = value;
                NotifyPropertyChanged("Age");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
