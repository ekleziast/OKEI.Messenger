using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextLibrary
{
    /// <summary>
    /// Коды запросов и ответов на сервер
    /// </summary>
    public enum Codes : int
    {
        False = 0,
        True = 1,
        /// <summary>
        /// Подтверждение
        /// </summary>
        Confirmation = 2,
        /// <summary>
        /// Регистрация
        /// </summary>
        Registraion = 3,
        /// <summary>
        /// Авторизация
        /// </summary>
        Authorization = 4,
        /// <summary>
        /// Новое сообщение
        /// </summary>
        NewMessage = 5,
        /// <summary>
        /// Новый диалог
        /// </summary>
        NewConversation = 6,
        /// <summary>
        /// В диалог добавлен новый участник
        /// </summary>
        NewMember = 7,
        /// <summary>
        /// Из диалога удален участник
        /// </summary>
        RemoveMember = 8,
        /// <summary>
        /// Участник покинул диалог
        /// </summary>
        LeaveMember = 9,
        /// <summary>
        /// Клиент разлогинился
        /// </summary>
        LogOut = 10,
        /// <summary>
        /// Клиент запросил список сообщений
        /// </summary>
        GetMessages = 11,
        /// <summary>
        /// Клиент запросил список диалогов
        /// </summary>
        GetConversations = 12,
        /// <summary>
        /// Клиент запросил список пользователей
        /// </summary>
        GetUsers = 13,
        /// <summary>
        /// Клиент установил новый статус
        /// </summary>
        NewStatus = 14,
    }
}
