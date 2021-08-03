using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Live.Avalonia.Sample.Library
{
    public class AppViewModel : ReactiveValidationObject
    {
        public AppViewModel()
        {
            this.ValidationRule(
                x => x.Name, 
                name => !string.IsNullOrWhiteSpace(name), 
                "Name shouldn't be empty.");

            Execute = ReactiveCommand.Create(
                () => Name = string.Empty,
                this.IsValid());
        }

        [Reactive]
        public string Name { get; set; }
        
        public ReactiveCommand<Unit, string> Execute { get; }
    }
}
