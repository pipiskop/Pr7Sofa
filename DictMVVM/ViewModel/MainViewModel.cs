using DictMVVM.Model;
using DictMVVM.View;
using DictMVVM.ViewModel.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DictMVVM.ViewModel
{
    internal class MainViewModel : BindingHelper
    {
        public WordModel NewWord;
        private string _newWordTerm;
        private string _newWordDefinition;
        private ObservableCollection<WordModel> _words;
        private string _searchTerm;
        private ICollectionView _wordsView;
        private string _jsonFilePath;
        public RelayCommand AddWordCommand { get; set; }
        public RelayCommand EditWordCommand { get; set; }
        public RelayCommand DeleteWordCommand { get; set; }
        public RelayCommand SaveWordCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand UpdateWordCommand { get; set; }


        public ObservableCollection<WordModel> Words
        {
            get { return _words; }
            set
            {
                _words = value;
                OnPropertyChanged();
            }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
                _wordsView.Refresh();
            }
        }
        public string NewWordTerm
        {
            get { return _newWordTerm; }
            set
            {
                _newWordTerm = value;
                OnPropertyChanged();
            }
        }

        public string NewWordDefinition
        {
            get { return _newWordDefinition; }
            set
            {
                _newWordDefinition = value;
                OnPropertyChanged();
            }
        }
        private WordModel _selectedWord;
        public WordModel SelectedWord
        {
            get { return _selectedWord; }
            set
            {
                _selectedWord = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel()
        {
            _jsonFilePath = GetJsonFilePath();
            LoadWordsFromJson();
            _wordsView = CollectionViewSource.GetDefaultView(Words);
            _wordsView.Filter = FilterWords;
            AddWordCommand = new RelayCommand(_ => AddWord());
            DeleteWordCommand = new RelayCommand(_ => DeleteWord());
            SaveWordCommand = new RelayCommand(_ => SaveWord());
            EditWordCommand = new RelayCommand(_ => EditWord());
            UpdateWordCommand = new RelayCommand(_ => UpdateWord());

        }
        private EditWord editWordWindow;
        private MainViewModel mainViewModel;
        private AddWord addWordWindow;
        private void AddWord()
        {
            mainViewModel = new MainViewModel();
            addWordWindow = new AddWord();
            addWordWindow.DataContext = mainViewModel;
            mainViewModel.WordAdded += AddWordWindow_WordAdded;
            mainViewModel.CancelCommand = new RelayCommand(_ => Cancel());
            addWordWindow.ShowDialog();
            //if (result == true)
            //{
            //    WordModel newWord = addViewModel.NewWord;
            //    Words.Add(newWord);
            //    SaveWordsToJson();
            //}
            //SaveWordsToJson();
        }
        private void Cancel()
        {
            addWordWindow.Close();
        }
        private void EditWord()
        {
            if (SelectedWord != null)
            {
                mainViewModel = new MainViewModel();
                editWordWindow = new EditWord();
                editWordWindow.DataContext = mainViewModel;
                mainViewModel.NewWordTerm = SelectedWord.Term; 
                mainViewModel.NewWordDefinition = SelectedWord.Definition; 
                mainViewModel.WordAdded += AddWordWindow_WordAdded;
                mainViewModel.CancelCommand = new RelayCommand(_ => Cancel());
                editWordWindow.ShowDialog();
                UpdateWordCommand.Execute(null);
            }
            else
            {
                MessageBox.Show("Не выбран элемент");
            }
        }
        private void UpdateWord()
        {
            if (SelectedWord != null)
            {
                if (!string.IsNullOrEmpty(mainViewModel.NewWordTerm) && !string.IsNullOrEmpty(mainViewModel.NewWordDefinition))
                {
                    SelectedWord.Term = mainViewModel.NewWordTerm;
                    SelectedWord.Definition = mainViewModel.NewWordDefinition;
                    SaveWordsToJson();
                    _wordsView.Refresh();
                }
                else MessageBox.Show("Поля не могут быть пустыми");
            }
        }
        private void SaveWord()
        {
            if (!string.IsNullOrEmpty(NewWordTerm) && !string.IsNullOrEmpty(NewWordDefinition))
            {
                NewWord = new WordModel
                {
                    Term = NewWordTerm,
                    Definition = NewWordDefinition
                };

                OnWordAdded(NewWord);
                _wordsView.Refresh();
            }
            else MessageBox.Show("Поля не могут быть пустыми");
        }
        private void AddWordWindow_WordAdded(WordModel word)
        {
            Words.Add(word);
            SaveWordsToJson();
        }
        public event Action<WordModel> WordAdded;
        protected virtual void OnWordAdded(WordModel word)
        {
            WordAdded?.Invoke(word);
        }

        private void DeleteWord()
        {
            if (SelectedWord != null)
            {
                Words.Remove(SelectedWord);
                SaveWordsToJson();
            }
            else
            {
                MessageBox.Show("Не выбран элемент");
            }
        }
        private void LoadWordsFromJson()
        {
            if (File.Exists(_jsonFilePath))
            {
                string json = File.ReadAllText(_jsonFilePath);
                Words = JsonConvert.DeserializeObject<ObservableCollection<WordModel>>(json);
            }
            else
            {
                Words = new ObservableCollection<WordModel>
                {
                    new WordModel { Term = "Яблоко", Definition = "Фрукт, который растет на дереве." },
                    new WordModel { Term = "Автомобиль", Definition = "Средство передвижения с 4-мя колесами." },
                    new WordModel { Term = "Книга", Definition = "Письменное или печатное произведение, состоящее из страниц, склеенных или сшитых вместе с одной стороны и переплетенных в обложки." }
                };
                SaveWordsToJson();
            }
        }
        public void SaveWordsToJson()
        {
            string json = JsonConvert.SerializeObject(Words);
            File.WriteAllText(_jsonFilePath, json);
        }
        private string GetJsonFilePath()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return Path.Combine(desktopPath, "words.json");
        }
        private bool FilterWords(object obj)
        {
            if (string.IsNullOrEmpty(SearchTerm))
                return true;

            WordModel word = obj as WordModel;
            return word.Term.ToLower().Contains(SearchTerm.ToLower());
        }
    }
}
