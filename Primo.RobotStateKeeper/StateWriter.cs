using LTools.Common.Exceptions;
using LTools.Common.Model;
using LTools.Common.UIElements;
using LTools.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Primo.RobotStateKeeper
{
    public class StateWriter : PrimoComponentTO<RobotStateWriterControl>
    {
        private const string CGroupName = "RobotStateKeeper";
        public override string GroupName
        {
            get => CGroupName;
            protected set { }
        }
        protected override int sdkTimeOut { get => 5000; set { } }

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

        //Prop In TransactionID
        private string _transsactionId;
        [LTools.Common.Model.Serialization.StoringProperty]
        [LTools.Common.Model.Studio.ValidateReturnScript(DataType = typeof(string))]
        [System.ComponentModel.Category("In"), System.ComponentModel.DisplayName("TransactionId")]
        public string TransactionId
        {
            get => this._transsactionId;
            set
            {
                _transsactionId = value;
                InvokePropertyChanged(this, "TransactionId");
            }
        }

        //Prop In Exception
        private string _exceptionInfo;
        [LTools.Common.Model.Serialization.StoringProperty]
        [LTools.Common.Model.Studio.ValidateReturnScript(DataType = typeof(LTools.Common.Model.ExecutionExceptionInfo))]
        [System.ComponentModel.Category("In"), System.ComponentModel.DisplayName("ExceptionInfo")]
        public string ExceptionInfo
        {
            get => this._exceptionInfo;
            set
            {
                _exceptionInfo = value;
                InvokePropertyChanged(this, "ExceptionInfo");
            }
        }

        //Prop In RobotState
        private string _robotState;
        [LTools.Common.Model.Serialization.StoringProperty]
        [LTools.Common.Model.Studio.ValidateReturnScript(DataType = typeof(RobotStateModel))]
        [System.ComponentModel.Category("In"), System.ComponentModel.DisplayName("RobotState")]
        public string RobotState
        {
            get => this._robotState;
            set
            {
                _robotState = value;
                InvokePropertyChanged(this, "RobotState");
            }
        }

        public StateWriter(IWFContainer container) : base(container)
        {
            sdkComponentName = "StateWriter";
            sdkComponentHelp = "Элемент записывает состояние робота в json";
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
                },
                new LTools.Common.Helpers.WFHelper.PropertiesItem()
                {
                    PropName = "ExceptionInfo",
                    PropertyType = LTools.Common.Helpers.WFHelper.PropertiesItem.PropertyTypes.SCRIPT,
                    EditorType = ScriptEditorTypes.NONE,
                    DataType = typeof(ExecutionExceptionInfo), ToolTip = "RobotStateKeeper", IsReadOnly = true
                },
                new LTools.Common.Helpers.WFHelper.PropertiesItem()
                {
                    PropName = "TransactionId",
                    PropertyType = LTools.Common.Helpers.WFHelper.PropertiesItem.PropertyTypes.SCRIPT,
                    EditorType = ScriptEditorTypes.NONE,
                    DataType = typeof(string), ToolTip = "RobotStateKeeper", IsReadOnly = true
                }                
            };
            InitClass(container);
        }   
        public override ExecutionResult TimedAction(ScriptingData sd)
        {            
            try
            {
                string jsonPath = GetPropertyValue<string>(JsonFilePath, "JsonFilePath", sd);
                string transactionId = GetPropertyValue<string>(TransactionId, "TransactionId", sd);
                RobotStateModel robotState = GetPropertyValue<RobotStateModel>(RobotState, "RobotState", sd);
                ExecutionExceptionInfo exceptionInfo = GetPropertyValue<ExecutionExceptionInfo>(ExceptionInfo, "ExceptionInfo", sd);
                if (robotState != null)
                {
                    UpdateRobotState(jsonPath, robotState);
                }
                else
                {
                    WriteException(jsonPath, exceptionInfo, transactionId);
                }
                
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
        //Write this Method
        private void UpdateRobotState(string jsonPath, RobotStateModel robotState)
        {
            List<RobotStateModel> existingStates = Read(jsonPath);
            var existingState = existingStates.FirstOrDefault(s => s.Id == robotState.Id);

            if (existingState != null)
            {
                // Обновляем существующее состояние
                existingState.CompletedSeccessfully = robotState.CompletedSeccessfully;
                existingState.RobotsErrors = robotState.RobotsErrors;
                existingState.DateCreated = robotState.DateCreated;
            }
            else
            {
                // Добавляем новое состояние
                existingStates.Add(robotState);
            }

            // Сериализация и запись обновленного списка состояний
            var updatedJson = JsonConvert.SerializeObject(existingStates, Formatting.Indented);
            File.WriteAllText(jsonPath, updatedJson);
        }

        private void WriteException(string jsonPath, ExecutionExceptionInfo exceptionInfo, string transactionId = "")
        {
            List<RobotStateModel> existingStates = Read(jsonPath);
            RobotStateModel lastState = existingStates.LastOrDefault(); // Получаем последнее состояние
            if (lastState != null && lastState.DateCreated == DateTime.UtcNow.Date)
            {
                // Проверяем, есть ли уже ошибка с таким transactionItemId
                RobotErrors existingError = null;
                if (!string.IsNullOrEmpty(transactionId))
                {
                    existingError = lastState.RobotsErrors.FirstOrDefault(e => e.TransactionItemId == transactionId);
                }
                if (existingError != null)
                {
                    // Если найдена ошибка с таким transactionItemId, обновляем её
                    existingError.ExceptionType = exceptionInfo.ExceptionType.ToString();
                    existingError.ExceptionMessage = exceptionInfo.Message;
                    existingError.PathOfTheSequence = exceptionInfo.WorkflowPath;
                    existingError.Block = exceptionInfo.SourceName;
                    existingError.TransactionItemId = transactionId;
                    existingError.TimeStamp = DateTime.UtcNow;
                }
                else
                {
                    // Если нет, добавляем новую ошибку
                    lastState.RobotsErrors.Add(new RobotErrors
                    {
                        ExceptionType = exceptionInfo.ExceptionType.ToString(),
                        ExceptionMessage = exceptionInfo.Message,
                        PathOfTheSequence = exceptionInfo.WorkflowPath,
                        Block = exceptionInfo.SourceName,
                        TransactionItemId = transactionId,
                        TimeStamp = DateTime.UtcNow
                    });
                }
            }
            else
            {
                // Если нет состояний или последнее состояние не сегодня, создаем новое
                var newRobotState = new RobotStateModel
                {
                    RobotsErrors = new List<RobotErrors>
                    {
                        new RobotErrors
                        {
                            ExceptionType = exceptionInfo.ExceptionType.ToString(),
                            ExceptionMessage = exceptionInfo.Message,
                            PathOfTheSequence = exceptionInfo.WorkflowPath,
                            Block = exceptionInfo.SourceName,
                            TransactionItemId = transactionId,
                            TimeStamp = DateTime.UtcNow
                        }
                    },
                    DateCreated = DateTime.UtcNow.Date // Устанавливаем сегодняшнюю дату
                };
                existingStates.Add(newRobotState);
            }
            // Сериализация и запись обновленного списка состояний
            var updatedJson = JsonConvert.SerializeObject(existingStates, Formatting.Indented);
            File.WriteAllText(jsonPath, updatedJson);
        }
    }
}
