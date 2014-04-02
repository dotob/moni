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
      vm.EditShortCut = new ShortcutViewModel(newSc, vm.WorkWeek, vm.Settings, null);
      vm.EditShortCut.SaveShortcut();

      var shortCut = vm.WorkWeek.Month.ShortCutStatistic.FirstOrDefault(s => s.Key.Equals(newSc.Key));
      Assert.NotNull(shortCut);
      Assert.AreEqual(newSc.Key, shortCut.Key);

      vm.SelectDate(vm.WorkWeek.StartDate.AddMonths(1));

      shortCut = vm.WorkWeek.Month.ShortCutStatistic.FirstOrDefault(s => s.Key.Equals(newSc.Key));
      Assert.NotNull(shortCut);
      Assert.AreEqual(newSc.Key, shortCut.Key);
    }

    [Test(Description = "#40 switch week from KW 14 to KW 15 in 2014")]
    public void ShouldSelectNextWeekOnMonthChange() {
      var vm = new MainViewModel(Dispatcher.CurrentDispatcher);

      vm.SelectDate(new DateTime(2014, 3, 31));

      Assert.AreEqual(2014, vm.WorkYear.Year);
      Assert.AreEqual(3, vm.WorkMonth.Month);
      Assert.AreEqual(31, vm.WorkWeek.StartDate.Day);

      vm.NextWeekCommand.Execute(null);

      Assert.AreEqual(2014, vm.WorkYear.Year);
      Assert.AreEqual(4, vm.WorkMonth.Month);
      Assert.AreEqual(1, vm.WorkWeek.StartDate.Day);
    }

    [Test(Description = "#40 switch week from KW 9 to KW 10 in 2014")]
    public void ShouldSelectNextWeekOnMonthChange2() {
      var vm = new MainViewModel(Dispatcher.CurrentDispatcher);

      vm.SelectDate(new DateTime(2014, 2, 28));

      Assert.AreEqual(2014, vm.WorkYear.Year);
      Assert.AreEqual(2, vm.WorkMonth.Month);
      Assert.AreEqual(24, vm.WorkWeek.StartDate.Day);

      vm.NextWeekCommand.Execute(null);

      Assert.AreEqual(2014, vm.WorkYear.Year);
      Assert.AreEqual(3, vm.WorkMonth.Month);
      Assert.AreEqual(1, vm.WorkWeek.StartDate.Day);
    }

    [Test(Description = "#40 switch last week from 2013 to 2014")]
    public void ShouldSelectNextWeekOnMonthChange3() {
      var vm = new MainViewModel(Dispatcher.CurrentDispatcher);

      vm.SelectDate(new DateTime(2013, 12, 31));

      Assert.AreEqual(2013, vm.WorkYear.Year);
      Assert.AreEqual(12, vm.WorkMonth.Month);
      Assert.AreEqual(30, vm.WorkWeek.StartDate.Day);

      vm.NextWeekCommand.Execute(null);

      Assert.AreEqual(2014, vm.WorkYear.Year);
      Assert.AreEqual(1, vm.WorkMonth.Month);
      Assert.AreEqual(1, vm.WorkWeek.StartDate.Day);
    }
  }
}