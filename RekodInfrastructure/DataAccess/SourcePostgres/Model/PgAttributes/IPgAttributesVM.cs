using System;
using PgAtM = Rekod.DataAccess.SourcePostgres.Model.PgAttributes;
//using PgAtVM = Rekod.DataAccess.SourcePostgres.ViewModel.PgAttributes;
using PgM = Rekod.DataAccess.SourcePostgres.Model;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Rekod.DataAccess.SourcePostgres.Model.PgAttributes
{
    public interface IPgAttributesVM
    {
        //PgAtVM.PgAttributesVM AttributeVM { get; }
        PgAtM.PgAttributeM PkAttribute { get; }
        ObservableCollection<PgAtM.PgAttributeM> Attributes { get; }
        bool IsNew { get; }
        bool IsReadOnly { get; }
        PgAtM.PgAttributeM FindAttribute(AbsM.IFieldM field);
        PgAtM.PgAttributeVariantM GetRefValue(PgAtM.PgAttributeM attribute);
        
        ICommand ReloadCommand { get; }
        ICommand SaveCommand { get; }
        ICommand OpenTableToSelectCommand { get; }
        ICommand OpenTableToViewCommand { get; }
        ICommand ClearValueInFieldCommand { get; }
    }
}
