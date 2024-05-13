using LTools.Common.Model;
using LTools.Common.UIElements;
using LTools.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Primo.RobotStateKeeper
{
    public class StateReader : PrimoComponentSimple<RobotStateReaderControl>
    {
        private const string CGroupName = "RobotStateKeeper";
        public override string GroupName
        {
            get => CGroupName;
            protected set { }
        }
        //protected override int sdkTimeOut { get => 10000; set { } }
        //Prop In JsonFilePath
        private string _jsonFilePath;
        [LTools.Common.Model.Serialization.StoringProperty]
        [LTools.Common.Model.Studio.ValidateReturnScript(DataType = typeof(string))]
        [System.ComponentModel.Category("In"), System.ComponentModel.DisplayName("JsonFilePath")]
        public string JsonFilePath
        {
            get => this._jsonFilePath;
            set
            {
                _jsonFilePath = value;
                InvokePropertyChanged(this, "JsonFilePath");
            }
        }
        
        //Prop Out RobotState
        private string _robotState;
        [LTools.Common.Model.Serialization.StoringProperty]
        [LTools.Common.Model.Studio.ValidateReturnScript(DataType = typeof(RobotStateModel))]
        [System.ComponentModel.Category("Out"), System.ComponentModel.DisplayName("RobotState")]
        public string RobotState
        {
            get => this._robotState;
            set
            {
                _robotState = value;
                InvokePropertyChanged(this, "RobotState");
            }
        }
        public StateReader(IWFContainer container) : base(container)
        {
            sdkComponentName = "StateReader";
            sdkComponentHelp = "Компонент, который считывает состояние робота из JSON-файла\r\nСвойства\r\n•\tJsonFilePath*: [String] Путь к JSON-файлу, который содержит состояние робота (например, c:\\folder\\state.json)\r\n•\tRobotState: [RobotStateModel] Самое последнее состояние робота, считанное из JSON-файла. Это свойство обновляется каждый раз, когда компонент считывает состояние из файла.\r\n";
            sdkProperties = new List<LTools.Common.Helpers.WFHelper.PropertiesItem>()
            {
                new LTools.Common.Helpers.WFHelper.PropertiesItem()
                {
                    PropName = "JsonFilePath",
                    PropertyType = LTools.Common.Helpers.WFHelper.PropertiesItem.PropertyTypes.SCRIPT,
                    EditorType = ScriptEditorTypes.FOLDER_SELECTOR,
                    DataType = typeof(string), ToolTip = "RobotStateKeeper", IsReadOnly = true
                },
                new LTools.Common.Helpers.WFHelper.PropertiesItem()
                {
                    PropName = "RobotState",
                    PropertyType = LTools.Common.Helpers.WFHelper.PropertiesItem.PropertyTypes.SCRIPT,
                    EditorType = ScriptEditorTypes.NONE,
                    DataType = typeof(RobotStateModel), ToolTip = "RobotStateKeeper", IsReadOnly = false
                }
            };
            InitClass(container);

        }
        public override ExecutionResult SimpleAction(ScriptingData sd)
        {
            try
            {
                string jsonPath = GetPropertyValue<string>(JsonFilePath, "JsonFilePath", sd);
                RobotStateModel stateModel = Read(jsonPath).OrderByDescending(state => state.DateCreated).FirstOrDefault();
                SetVariableValue<RobotStateModel>(RobotState, stateModel, sd);
                return new ExecutionResult() { IsSuccess = true, SuccessMessage = "Done" };
            }
            catch (Exception ex)
            {
                return new ExecutionResult() { IsSuccess = false, ErrorMessage = ex?.Message };
            }
        }

        private static List<RobotStateModel> Read(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new List<RobotStateModel>();
                }
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<RobotStateModel>>(json) ?? new List<RobotStateModel>();
            }
            catch
            {
                // Log the error or handle it appropriately
                return new List<RobotStateModel>();
            }
        }
    }
}
