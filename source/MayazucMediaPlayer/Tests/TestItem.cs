using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI;
using System.Threading.Tasks;
using Windows.UI;

namespace MayazucMediaPlayer.Tests
{
    public class TestItem : ObservableObject
    {

        bool isTestRunnable = true;
        public bool IsTestRunnable
        {
            get
            {
                return isTestRunnable;
            }
            set
            {
                isTestRunnable = value;
                NotifyPropertyChanged(nameof(IsTestRunnable));
            }
        }

        Color _TestResultColor;
        public Color TestResultColor
        {
            get
            {
                return _TestResultColor;
            }
            set
            {
                _TestResultColor = value;
                NotifyPropertyChanged(nameof(TestResultColor));
            }
        }

        public string TestName
        {
            get;
            private set;
        }


        public CommandBase TestCommand
        {
            get;
            private set;
        }



        public TestItem(string name, CommandBase command)
        {
            TestName = name;
            TestCommand = command;
            TestResultColor = Colors.Gray;
        }

        public async Task<bool> ExecuteTest()
        {
            if (IsTestRunnable)
            {
                try
                {
                    IsTestRunnable = false;
                    TestResultColor = Colors.Yellow;
                    await TestCommand.ExecuteAsync(null);
                    TestResultColor = Colors.Green;
                    return true;
                }
                catch
                {
                    TestResultColor = Colors.Red;
                    return false;
                }
                finally
                {
                    IsTestRunnable = true;
                }
            }
            else return false;
        }
    }
}
