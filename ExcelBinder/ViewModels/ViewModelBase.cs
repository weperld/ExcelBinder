using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExcelBinder.ViewModels
{
    /// <summary>
    /// MVVM 패턴을 위한 ViewModel 기본 클래스입니다.
    /// 속성 변경 통지 및 비동기 작업 상태 관리 기능을 제공합니다.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isBusy;

        /// <summary>
        /// 현재 백그라운드 작업이 진행 중인지 여부를 나타냅니다.
        /// 이 속성은 UI의 로딩 오버레이와 바인딩되어 사용자 조작을 제어합니다.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
