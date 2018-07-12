using System;
using AbsM = Rekod.DataAccess.AbstractSource.Model;
namespace Rekod.DataAccess.TableManager.Model
{
    public interface ITableManagerVM
    {
        void AddRepository(Rekod.DataAccess.AbstractSource.Model.IDataRepositoryM repository);
        AxmvMapLib.AxMapLIb mv { get; }
        System.Windows.Input.ICommand ReloadRepositoriesCommand { get; }
        AbsM.ILayerM EditableLayer { get; }
        AbsM.ILayerM SelectedLayer { get; }
    }
}