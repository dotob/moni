using System;
using System.Linq;
using System.Windows.Threading;
using MONI.Data;
using MONI.ViewModels;
using NUnit.Framework;

namespace MONI.Tests
{
  [TestFixture]
  public class MainViewModelTester
  {
    [Test]
    public void CreatedShortCutShouldShownInNextMonth() {
      var vm = new MainViewModel(Dispatcher.CurrentDispatcher);

      var newSc = new ShortCut();
      newSc.Key = newSc.ID;
      newSc.Expansion = "8,8;12345-000";
      newSc.ValidFrom = DateTime.Now;
      vm.EditShortCut = newSc;
      vm.SaveEditShortcut();

      var shortCut = vm.WorkWeek.Month.ShortCutStatistic.FirstOrDefault(s => s.Key.Equals(newSc.Key));
      Assert.NotNull(shortCut);
      Assert.AreEqual(newSc.Key, shortCut.Key);

      vm.SelectDate(vm.WorkWeek.StartDate.AddMonths(1));

      shortCut = vm.WorkWeek.Month.ShortCutStatistic.FirstOrDefault(s => s.Key.Equals(newSc.Key));
      Assert.NotNull(shortCut);
      Assert.AreEqual(newSc.Key, shortCut.Key);
    }
  }
}