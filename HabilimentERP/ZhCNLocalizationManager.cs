using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;

namespace HabilimentERP
{
    public class ZhCNLocalizationManager : LocalizationManager
    {
        public override string GetStringOverride(string key)
        {
            switch (key)
            {
                //Filter & GridView
                case "FilterIsEqualTo":
                case "GridViewFilterIsEqualTo":
                    return "等于";
                case "GridViewFilterIsNotEqualTo":
                case "FilterIsNotEqualTo":
                    return "不等于";
                case "FilterAnd":
                    return "同时满足";
                case "FilterContains":
                case "GridViewFilterContains":
                    return "包含";
                case "FilterDoesNotContain":
                case "GridViewFilterDoesNotContain":
                    return "不包含";
                case "FilterEditorFormatExceptionMessage":
                    return "输入格式不正确";
                case "FilterEndsWith":
                case "GridViewFilterEndsWith":
                    return "以..结尾";
                case "FilterIsContainedIn":
                case "GridViewFilterIsContainedIn":
                    return "被包含于";
                case "FilterIsEmpty":
                case "GridViewFilterIsEmpty":
                    return "为空";
                case "GridViewFilterIsGreaterThan":
                case "FilterIsGreaterThan":
                    return "大于";
                case "GridViewFilterIsGreaterThanOrEqualTo":
                case "FilterIsGreaterThanOrEqualTo":
                    return "大于等于";
                case "FilterIsLessThan":
                case "GridViewFilterIsLessThan":
                    return "小于";
                case "GridViewFilterIsLessThanOrEqualTo":
                case "FilterIsLessThanOrEqualTo":
                    return "小于等于";
                case "GridViewFilterIsNotContainedIn":
                case "FilterIsNotContainedIn":
                    return "不包含于";
                case "GridViewFilterIsNotEmpty":
                case "FilterIsNotEmpty":
                    return "不为空";
                case "GridViewFilterIsNotNull":
                case "FilterIsNotNull":
                    return "有效";
                case "GridViewFilterIsNull":
                case "FilterIsNull":
                    return "无效";
                case "GridViewFilterMatchCase":
                case "FilterMatchCase":
                    return "区分大小写";
                case "FilterOr":
                    return "满足其一";
                case "GridViewFilterStartsWith":
                case "FilterStartsWith":
                    return "以..开头";
                case "GridViewAlwaysVisibleNewRow":
                    return "点击此处新增行";
                case "GridViewClearFilter":
                    return "清除筛选条件";
                case "GridViewFilter":
                    return "筛选";
                case "GridViewFilterAnd":
                    return "并且";
                case "GridViewFilterDistinctValueNull":
                    return "[无效]";
                case "GridViewFilterDistinctValueStringEmpty":
                    return "[空]";
                case "GridViewFilterOr":
                    return "或者";
                case "GridViewFilterSelectAll":
                    return "选择全部";
                case "GridViewFilterShowRowsWithValueThat":
                    return "显示相应行";
                case "GridViewGroupPanelText":
                    return "拖拽列头到此处可按该列分组显示";
                case "GridViewGroupPanelTopText":
                    return "分组标头";
                case "GridViewGroupPanelTopTextGrouped":
                    return "以..分组";

                case "Cancel":
                    return "取消";
                case "Ok":
                case "Confirm":
                    return "确定";
                case "Maximize":
                    return "最大化";
                case "Minimize":
                    return "最小化";
                case "EnterDate":
                    return "选择日期";
                case "Error":
                    return "错误";

                //DataForm
                case "DataForm_AddNew":
                    return "新增";
                case "DataForm_BeginEdit":
                    return "编辑";
                case "DataForm_Delete":
                    return "删除";
                case "DataForm_MoveCurrentToFirst":
                    return "回到第一项";
                case "DataForm_MoveCurrentToLast":
                    return "去到最后项";
                case "DataForm_MoveCurrentToNext":
                    return "下一项";
                case "DataForm_MoveCurrentToPrevious":
                    return "前一项";
            }
            return base.GetStringOverride(key);
        }
    }
}
