using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Services;
using Microsoft.UI;
using System.Threading.Tasks;
using Windows.UI;

namespace MayazucMediaPlayer.Tests
{
    public partial class TestItem : ObservableObject
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
                if (isTestRunnable == value) return;

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
                if (_TestResultColor == value) return;

                _TestResultColor = value;
                NotifyPropertyChanged(nameof(TestResultColor));
            }
        }

        public string TestName
        {
            get;
            private set;
        }


        public AsyncRelayCommand<object> TestCommand
        {
            get;
            private set;
        }



        public TestItem(string name, AsyncRelayCommand<object> command)
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
