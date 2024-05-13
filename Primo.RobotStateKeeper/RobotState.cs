using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RobotState

{    
    public class JsonStateHandler
    {
        /// <summary>
        /// Читает свсе состояния робота из файла
        /// </summary>
        /// <param name="filePath">Путь к файлу состояния робота json</param>
        /// <returns>Список всех состояниий робота</returns>
        public static List<RobotStateModel> Read(string filePath)
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
        /// <summary>
        /// Возвращает состояние робота на текущий день
        /// </summary>
        /// <param name="filePath">Путь к файлу состояния робота json</param>
        /// <returns>Состояние робота за текущий день или null, если таковое отсутсвует</returns>
        public static RobotStateModel ReadLastState(string filePath)
        {
            var states = Read(filePath);
            return states.FirstOrDefault(state => state.DateCreated == DateTime.UtcNow.Date);
        }
        /// <summary>
        /// Записывает ошибку в состояние робота. Создает новое состояние за текущую дату, если его нет
        /// </summary>
        /// <param name="filePath">Путь к файлу состояния робота json</param>
        /// <param name="exceptionType">Тип исключения</param>
        /// <param name="exceptionMessage">Сообщение исключения</param>
        /// <param name="pathOfTheSequence">Путь к последовательности, в которой произошла ошибка</param>
        /// <param name="isNotificationSent">Отправлено ли уведомление</param>
        /// <param name="isTransaction">Является ли ошибка транзакционной</param>
        /// <param name="transactionItemId">Id транзакции</param>
        /// <param name="isNotificationSent">Отправлено ли уведомление</param>  
        /// <remarks>Если transactionItemId не пуст и сущестует ошибка с таким же ID в текущем состоянии, то ошибка будет обновлена</remarks>
        /// <returns></returns>
        public static void Write(string filePath, string exceptionType, string exceptionMessage, string pathOfTheSequence = "", bool isNotificationSent = false, bool isTransaction = false, string transactionItemId = "", int tryToRepead = 0)
        {
            List<RobotStateModel> existingStates = Read(filePath);
            RobotStateModel lastState = existingStates.LastOrDefault(); // Получаем последнее состояние
            if (lastState != null && lastState.DateCreated == DateTime.UtcNow.Date)
            {
                // Проверяем, есть ли уже ошибка с таким transactionItemId
                RobotsError existingError = null;
                if (isTransaction && !string.IsNullOrEmpty(transactionItemId))
                {
                    existingError = lastState.RobotsErrors.FirstOrDefault(e => e.TransactionItemId == transactionItemId);
                }
                if (existingError != null)
                {
                    // Если найдена ошибка с таким transactionItemId, обновляем её
                    existingError.ExceptionType = exceptionType;
                    existingError.ExceptionMessage = exceptionMessage;
                    existingError.PathOfTheSequence = pathOfTheSequence;
                    existingError.IsNotyficationSend = isNotificationSent;
                    existingError.TrysToRepead = tryToRepead;
                    existingError.TimeStamp = DateTime.UtcNow;
                }
                else
                {
                    // Если нет, добавляем новую ошибку
                    lastState.RobotsErrors.Add(new RobotsError
                    {
                        ExceptionType = exceptionType,
                        ExceptionMessage = exceptionMessage,
                        PathOfTheSequence = pathOfTheSequence,
                        IsNotyficationSend = isNotificationSent,
                        IsTransaction = isTransaction,
                        TransactionItemId = transactionItemId,
                        TrysToRepead = tryToRepead,
                        TimeStamp = DateTime.UtcNow
                    });
                }
            }
            else
            {
                // Если нет состояний или последнее состояние не сегодня, создаем новое
                var newRobotState = new RobotStateModel
                {
                    RobotsErrors = new List<RobotsError>
                    {
                        new RobotsError
                        {
                            ExceptionType = exceptionType,
                            ExceptionMessage = exceptionMessage,
                            PathOfTheSequence = pathOfTheSequence,
                            IsNotyficationSend = isNotificationSent,
                            IsTransaction = isTransaction,
                            TransactionItemId = transactionItemId,
                            TimeStamp = DateTime.UtcNow
                        }
                    },
                    DateCreated = DateTime.UtcNow.Date // Устанавливаем сегодняшнюю дату
                };
                existingStates.Add(newRobotState);
            }
            // Сериализация и запись обновленного списка состояний
            var updatedJson = JsonConvert.SerializeObject(existingStates, Formatting.Indented);
            File.WriteAllText(filePath, updatedJson);
        }
        // Перегруженный метод, использующий объект исключения
        public static void WriteException(string filePath, Exception exception, string pathOfTheSequence = "", bool isNotificationSent = false, bool isTransaction = false, string transactionItemId = "")
        {
            Write(filePath, exception.GetType().Name, exception.Message, pathOfTheSequence, isNotificationSent, isTransaction, transactionItemId);
        }
    }
}
