using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace winformThreadTest
{
    public class Service
    {
        public List<string> GetStrings()
        {
            List<string> stringList = new List<string>();

            Thread.Sleep(3000);

            stringList.Add("Name = Kacy");
            stringList.Add("Name = Tom");
            stringList.Add("Name = Henry");

            return stringList;
        }


        public async Task<List<string>> GetStringsAsync()
        {
            List<string> stringList = new List<string>();

            await GetAwaitData(stringList);

            return stringList;
        }

        private Task GetAwaitData(List<string> stringList)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(3000);

                stringList.Add("Name = Kacy");
                stringList.Add("Name = Tom");
                stringList.Add("Name = Henry");
            });
        }
    }
}