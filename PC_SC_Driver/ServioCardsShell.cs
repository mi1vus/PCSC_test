//using ProjectSummer.Repository;
using System;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using CoverConstants;
using Binding = System.ServiceModel.Channels.Binding;
using OpCode = CoverConstants.OpCode;

namespace ServioBonus
{
    [ServiceContract]
    public interface IServioOnlineAPI
    {
        [OperationContract]
        int GetCardType(int issuerId);
        [OperationContract]
        string GetCardInfo(int issuerId, string cardNumber, out int cardType);
        [OperationContract]
        int Debit(long transID, int issuerId, string cardNumber, int fuelCode, ref decimal amount, ref decimal price, ref decimal quantity, ref string slip, ref string error);
        [OperationContract]
        int Commit(long transID, int issuerId, string cardNumber, int fuelCode, ref decimal amount, ref decimal price, ref decimal quantity, ref string slip, ref string error, bool fullFilling = false);
        [OperationContract]
        decimal GetBonusBalance(int issuerId, string cardNumber, string product, int price);
        [OperationContract]
        int BonusCancel(long transID, int issuerId, string cardNumber, ref string error);
        [OperationContract]
        int BonusDebit(long transID, int issuerId, string cardNumber, int fuelCode, decimal amount, decimal price, decimal quantity, decimal bonus, ref string error);
        [OperationContract]
        int BonusCommit(long transID, int issuerId, string cardNumber, int fuelCode, decimal amount, decimal price, decimal quantity, decimal bonus, ref string error);
    }
    public class IServioOnlineAPIClient: ClientBase<IServioOnlineAPI>
    {
        public IServioOnlineAPIClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }
    }
    public unsafe class ServioCardsShell
    {
        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Init([MarshalAs(UnmanagedType.LPWStr)]string paramsFile, ref IntPtr objRef);

        //[DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        //private static extern int Deinit(ref IntPtr objRef);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern IntPtr CardOperation_Create();

        //[DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        //private static extern void CardOperation_Free(IntPtr operation);
        
        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int ReadCard(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, bool cardInfoOnly);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int WriteCard(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Auth(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int CardInfo(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Sale(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Credit(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Return(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        //[DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        //private static extern int Refund(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage/*, IntPtr operation*/);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CardOperation
        {
            /// <summary>
            /// Размер структуры
            /// </summary>
            public int Size;
            /// <summary>
            /// Номер АЗС
            /// </summary>
            public int POS;

            private IntPtr posName;
            /// <summary>
            /// Название АЗС
            /// </summary>
            public string PosName
            {
/*
                get { if (posName == IntPtr.Zero) return null; else return Marshal.PtrToStringBSTR(posName); }
*/
                set { posName = value == null ? IntPtr.Zero : Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// OLE Дата/Время проведения транзакции. 0-не заполнено
            /// </summary>
            public double OpTime;
            /// <summary>
            /// Код транзакции POS
            /// </summary>
            public long TransactID;
            /// <summary>
            /// Код операции: 0-продажа, 1-возврат
            /// </summary>
            public byte OpCode;
            /// <summary>
            ///  Операция по терминалу проводится уже после налива. Это накладывает ограничение на применение скидок ("пересчет денежного заказа, например")
            /// </summary>
            public byte IsPostpay;
            /// <summary>
            /// Тип карты 0-платежные карты, 1-карты лояльности
            /// </summary>
            public int CardType;
            /// <summary>
            /// Серийный (аппаратный) номер карты
            /// </summary>
            public SerialnumberT SerialNumber;
            /// <summary>
            /// Код эмитента
            /// </summary>
            public int IssuerID;

            private IntPtr cardNumber;
            /// <summary>
            /// Код карты VARCHAR(50)
            /// </summary>
            public string CardNumber
            {
                get { return cardNumber == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(cardNumber); }
                set { cardNumber = value == null ? IntPtr.Zero : Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// Клиент
            /// </summary>
            private readonly int HolderID;
            /// <summary>
            /// Добавлять ведущие нули к карте
            /// </summary>
            public byte AddPrefixZeros;
            /// <summary>
            /// Операция проводится без карты
            /// </summary>
            public byte WoCard;
            /// <summary>
            /// ПИН-код проверен
            /// </summary>
            public byte PINChecked;
            /// <summary>
            /// Используется мультиэмитентная карта
            /// </summary>
            private readonly byte UseMultiIssuer;
            /// <summary>
            /// Кол-во позиций
            /// </summary>
            public int ItemCount;
            /// <summary>
            /// Позиции операции
            /// </summary>
            public IntPtr Items;

            private readonly IntPtr checkImage;
            /// <summary>
            /// Образ квитанции VARCHAR(MAX_CHECK_IMAGE_LEN)
            /// </summary>
            public string CheckImage
            {
                get { return checkImage == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(checkImage); }
/*
                set { if (value == null) checkImage = IntPtr.Zero; else checkImage = Marshal.StringToBSTR(value); }
*/
            }
            /// <summary>
            /// Причина отказа по операции
            /// </summary>
            private readonly int DenyReason;
            /// <summary>
            /// Образ карты был изменен в процессе операции. Флаг используется внутри.
            /// </summary>
            public readonly byte CardImageChanged;
            /// <summary>
            ///  Использовать серийный номер в качестве номера карты
            /// </summary>
            private readonly byte SerialNumberAsIdentifier;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CardOperationItem
        {
            /// <summary>
            /// Размер структуры
            /// </summary>
            public int Size;
            /// <summary>
            /// Тип товара: 0-товары,1-услуги,2-топливо
            /// </summary>
            public byte GoodKind;
            private IntPtr goodCode;
            /// <summary>
            /// Код товара
            /// </summary>
            public string GoodCode
            {
                get { return goodCode == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(goodCode); }
                set { goodCode = value == null ? IntPtr.Zero : Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// Группа
            /// </summary>
            public int Group;
            private IntPtr goodName;
            /// <summary>
            /// Название
            /// </summary>
            public string GoodName
            {
                get { return goodName == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(goodName); }
                set { goodName = value == null ? IntPtr.Zero : Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// Номер ТРК. 0-магазин
            /// </summary>
            public byte FuellingPoint;
            /// <summary>
            /// Код продукта списания
            /// </summary>
            public int ProductID;
            /// <summary>
            /// Единицы списания с кошелька(!): 0-количество, 1-сумма
            /// </summary>
            public int UnitID;
            /// <summary>
            /// Номер использованного кошелька
            /// </summary>
            public int PurseNumber;
            /// <summary>
            /// Единицы заказа(!): 0-количество, 1-сумма
            /// </summary>
            public byte OrderUnit;
            /// <summary>
            /// Цена
            /// </summary>
            public double Price;
            ///Количество (Объем) 
            public double Quantity;
            /// <summary>
            /// Скидка с цены в %
            /// </summary>
            public double PriceDiscountPercent;
            /// <summary>
            /// Скидка с цены в р.
            /// </summary>
            public double PriceDiscountAbsolute;
            /// <summary>
            /// Скидка с цены в % по факту
            /// </summary>
            public double FactDiscountPercent;
            /// <summary>
            /// Скидка с цены в р. по факту 
            /// </summary>
            public double FactDiscountAbsolute;
            /// <summary>
            /// Причина отказа по позиции
            /// </summary>
            public int DenyReason;
            /// <summary>
            /// Используется постоянная скидка. По факту остается.
            /// </summary>
            public bool ConstantDiscount;
            /// <summary>
            /// Копейки округления
            /// </summary>
            public double RoundOff;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct SerialnumberT
        {
            [FieldOffset(0)] private readonly uint FourByteUID;
            [FieldOffset(0)] private fixed byte SevenByteUID[7];

            //public Tuple <string, string> GetSerial()
            //{
            //    var res1 = BitConverter.GetBytes(FourByteUID);
            //    var res2 = new byte[7];

            //    fixed (byte* p = SevenByteUID)
            //    {
            //        for (int i = 0; i < 7; ++i)
            //        {
            //            res2[i] = *(p + i);
            //        }
            //    }
            //    return new Tuple<string, string>(BitConverter.ToString(res1), BitConverter.ToString(res2));
            //}
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct Mf1S70T
        {
            public const int Size = 4096;

            [FieldOffset(0)] private fixed byte block[Size];

            public byte[] GetBlocks()
            {
                var res = new byte[Size];
                fixed (byte* p = block)
                {
                    for (var i = 0; i < Size; ++i)
                    {
                        res[i] = *(p + i);
                    }
                }
                return res;
            }

/*
            public void ClearBlocks()
            {
                fixed (byte* p = this.block)
                {
                    for (int i = 0; i < Size; ++i)
                    {
                        *(p + i) = 0xA0;
                    }
                }
            }
*/
        }

        public struct ServioCardInfo
        {
            public int ErrorCore;
            public string ErrorDescription => ((ErrorCodeDescriptions) ErrorCore).ToString();

            public string CardInfo { get; set; }

            public string CardNumber;
            public int IssuerID;

            public static ServioCardInfo operator +(ServioCardInfo obj1, ServioCardInfo obj2)
            {
                if (obj2.ErrorCore != (int) ErrorCodes.E_SUCCESS)
                    obj1.ErrorCore = obj2.ErrorCore;

                obj1.CardNumber = obj2.CardNumber;
                obj1.IssuerID = obj2.IssuerID;
                obj1.CardInfo += "\r\n\r\n" + obj2.CardInfo;
                return obj1;
            }

            public override string ToString()
            {
                return (ErrorCore != 0) ? $"Ошибка чтения карты: {ErrorCore} {ErrorDescription}" : $"Эмитент: {IssuerID}\r\nНомер карты: {CardNumber}";
            }
        }

        private static readonly string _config = @"C:\ServioCardAPI\SDK\Mifaread3.ini";

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int SetCallback(IntPtr obj, IntPtr callBack, IntPtr userData);
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CustomInteraction
        {
            private readonly int Size;            // Размер структуры
            public readonly int CIType;          // Тип инетрфейса
            public int ReturnCode;      // Результат выполнения: 0-ок, 1-ошибка, 2-отмена
            private readonly IntPtr Msg;             // Сообщение
            private readonly IntPtr Title;           // Заголовок окна
            private readonly int Flags;           // Флаги MessageBox / InputBox и т.д.
            private readonly uint Timeout;         // Таймаут показа диалогового окна
            private readonly int ProgressTotal;   // Макс.значение ProgressBar. Если ProgressTotal <= 0 То нужно просто показать процесс длительной операции, иначе можно ProgressBar.
            private readonly int ProgressCurrent; // Текущее значение ProgressBar. Если ProgressCurrent >= ProgressTotal то ProgressBar можно спрятать (операция завершена) иначе происходит выполнение операции.
            public readonly IntPtr Buffer;          // Строка с резултатом ввода (InputBox)
            public readonly int BufLen;          // Макс.длина буфера
            private readonly int ItemCount;       // Кол-во позиций ListBox
            private readonly IntPtr Items;         // Позиции ListBox
            private readonly int ItemIndex;       // Индекс выбранной позиции
        }
        private static string _pin = "";
        private static readonly CustomInteractionCallbackFuncDelegate Callback = CustomInteractionCallbackFunc;
        private delegate int CustomInteractionCallbackFuncDelegate(IntPtr ci, IntPtr userData);
        private static int CustomInteractionCallbackFunc(IntPtr ci, IntPtr userData)
        {

            CustomInteraction str = (CustomInteraction)Marshal.PtrToStructure(ci, typeof(CustomInteraction));

            if (str.CIType == 103)
            {
                System.Windows.Forms.MessageBox.Show($@"str.CIType = {str.CIType}, pin = {_pin}, str.BufLen = {str.BufLen}");
                var data = Encoding.Unicode.GetBytes(_pin + "\0");
                for (int z = 0; z < data.Length && z < str.BufLen; z++)
                    Marshal.WriteByte(str.Buffer, z, data[z]);
                //str.Buffer = Marshal.StringToHGlobalUni(pin);
                //str.BufLen = pin.Length;//data.Length;
                str.ReturnCode = 1;
            }
            Marshal.StructureToPtr(str, ci, true);

            return 0;
        }

        public static ServioCardInfo ReadCardNumber(string pin)
        {
            ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            try
            {
                _pin = pin;
                IntPtr obj = new IntPtr();

                if ((res.ErrorCore = Init(_config, ref obj)) != 0)
                    throw new Exception("Error Init No" + res);

                SetCallback(obj, Marshal.GetFunctionPointerForDelegate(Callback), IntPtr.Zero);
                SerialnumberT serialNumber = new SerialnumberT();
                IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
                IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
                IntPtr operationPtr = CardOperation_Create();
                var operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));

                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, true)) != 0)
                    throw new Exception("Error ReadCard No" + res);

                operation.PosName = "TestTestTest";
                operation.Size = sizeof(CardOperation);
                operation.POS = 1;
                operation.OpTime = DateTime.Now.ToOADate();
                operation.IsPostpay = 0;
                operation.CardType = (int)CardType.CT_IDENTIFIER_AUTHORIZATION;
                operation.SerialNumber = serialNumber;
                operation.CardNumber = null;
                operation.IssuerID = -1;
                operation.AddPrefixZeros = 0;
                operation.WoCard = 0;
                operation.PINChecked = (byte)(pin == ""? 1 : 0);
                operation.ItemCount = 0;
                Marshal.StructureToPtr(operation, operationPtr, true);

                //Operation = (TCardOperation)Marshal.PtrToStructure(_Operation, typeof(TCardOperation));

                if ((res.ErrorCore = Auth(obj, serialNumberPtr, cardImagePtr, operationPtr)) != 0)
                    throw new Exception("Error Auth No" + res);

                operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber.PadLeft(20, '0');
            }
            finally
            {
                _pin = "";
            }
            return res;
        }
        public static ServioCardInfo ReadCardNumber()
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            if ((res.ErrorCore = Init(_config, ref obj)) != 0)
                throw new Exception("Error Init No" + res);

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();
            try
            {
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, true)) != 0)
                    throw new Exception("Error ReadCard No" + res);
                //Operation = MarshalHelper.UnMemory<TCardOperation>.ReadInMem(OperationPtr);
                //var op = Operation.CheckImage;
                //SERIALNUMBER_T SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
                //var ser = SerialNumber.getSerial();

                //Operation.POSName = "TestTestTest";
                //Operation.Size = sizeof(TCardOperation);
                //Operation.POS = 1;
                //Operation.OpTime = DateTime.Now.ToOADate();
                //Operation.IsPostpay = 0;
                //Operation.CardType = 2;
                //Operation.SerialNumber = SerialNumber;
                //Operation.CardNumber = null;
                //Operation.IssuerID = -1;
                //Operation.AddPrefixZeros = 0;
                //Operation.WoCard = 0;
                //Operation.PINChecked = 1;
                //Operation.ItemCount = 0;
                //MarshalHelper.UnMemory<TCardOperation>.SaveInMem(Operation, ref OperationPtr);

                if ((res.ErrorCore = Auth(obj, serialNumberPtr, cardImagePtr, operationPtr)) != 0)
                    throw new Exception("Error Auth No" + res);

                var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                //var op = Operation.CheckImage;
                //SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
                //var ser = SerialNumber.getSerial();

                //Operation = (TCardOperation)Marshal.PtrToStructure(OperationPtr, typeof(TCardOperation));
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber.PadLeft(20, '0');
            }
            finally
            {
                MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
                MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                //CardOperation_Free(operationPtr);
            }
            return res;
        }
        // АВТОРИЗАЦИЯ
        public static ServioCardInfo Authorize()
        {
            ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();

            try
            {
                if ((res.ErrorCore = Init(_config, ref obj)) != (int)ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error Init No" + res);
                }
                //Thread.Sleep(1000);
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, true)) != (int) ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int) ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error ReadCard No" + res);
                }

                var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                SerialnumberT serialNumber = MarshalHelper.UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

                operation.Size = sizeof(CardOperation);
                operation.POS = 1; // Номер АЗС или терминала
                operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
                operation.TransactID = 0; // Тут желательно ваш код код транзакции указать
                operation.OpCode = (byte)OpCode.OP_AUTHORIZATION;
                operation.IsPostpay = 0; // как есть - если постоплата - указать true
                operation.CardType = (int)CardType.UNKNOWN; // Предположим что тип карты неизвестен
                operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
                operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                operation.CardNumber = ""; // Не указывать !!!
                operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                operation.WoCard = 0; //
                operation.PINChecked = 0; // Если указать false то драйвер запросит форму ввода пароля через callback в случае, если на карту поставили PIN-код
                operation.ItemCount = 0; // Позиции не нужны

                MarshalHelper.UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

                if ((res.ErrorCore = Auth(obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error Auth No" + res);
                }
                operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber.PadLeft(20, '0');
                return res;
            }
            finally
            {
                MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
                MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                //CardOperation_Free(operationPtr);
            }
        }

        public static byte[] GetCard()
        {
            ServioCardInfo err = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            if ((err.ErrorCore = Init(_config, ref obj)) != 0)
                throw new Exception("Error Init No" + err);

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
            if ((err.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != 0)
                throw new Exception("Error ReadCard No" + err);

            var blocks = MarshalHelper.UnMemory<Mf1S70T>.ReadInMem(cardImagePtr);
            var card = blocks.GetBlocks();

            return card;
        }
        // ИНФОРМАЦИЯ ПО КАРТЕ
        public static ServioCardInfo GetCardInfo()
        {
            ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();

            try
            {
                if ((res.ErrorCore = Init(_config, ref obj)) != (int)ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error Init No" + res);
                }
                MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int)ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error ReadCard No" + res);
                }
                var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                SerialnumberT serialNumber = MarshalHelper.UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

                operation.Size = sizeof(CardOperation);
                operation.POS = 1; // Номер АЗС или терминала
                operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
                operation.TransactID = 0; // Тут желательно ваш код код транзакции указать
                operation.CardType = (int)CardType.UNKNOWN; // Предположим что тип карты неизвестен
                operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
                operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                operation.CardNumber = ""; // Не указывать !!!
                operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                operation.ItemCount = 0; // При аутентификации позиции не заполнять.

                MarshalHelper.UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

                // Смотрим информацию по карте
                if ((res.ErrorCore = CardInfo(obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.E_SUCCESS)
                    throw new Exception("Error CardInfo No" + res);

                // флаг указывает на то что образ карты был изменен в процессе операции
                // и нужно записать его обратно.
                // Нужно для пополнения карты при просмотре информации
                operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                if (operation.CardImageChanged != 0)
                {
                    // при ошибке можно попробовать повторить операцию несколько раз 3-10...
                    // если не проходит то это патовая ситуация и для её дальнейшего
                    // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
                    // В теории, сохранённый SerialNumber и CardImage можно попробовать
                    // записать обратно даже при перезапуске ПО, но до следующего
                    // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
                    // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
                    if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.E_SUCCESS)
                        throw new Exception("Error WriteCard No" + res);
                }

                operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
                res.CardInfo = operation.CheckImage;
                return res;
            }
            finally
            {
                MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
                MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                //CardOperation_Free(operationPtr);
            }

        }

        /// <summary>
        /// ОПЕРАЦИЯ ПО КАРТЕ
        /// </summary>
        /// <param name="type">тип операции</param>
        /// <param name="terminal">терминал (азс)</param>
        /// <param name="transact">номер транзакции</param>
        /// <param name="opItem">параметры операции</param>
        /// <returns></returns>
        public static ServioCardInfo CardOperationExecute(CardOperationType type, int terminal, int transact, CardOperationItem opItem)
        {
            ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();

            try
            {
                if ((res.ErrorCore = Init(_config, ref obj)) != (int) ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int) ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error Init No" + res);
                }
                MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int) ErrorCodes.E_SUCCESS)
                {
                    if (res.ErrorCore == (int) ErrorCodes.E_CANCEL)
                    {
                        return res;
                    }
                    throw new Exception("Error ReadCard No" + res);
                }
                var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                SerialnumberT serialNumber = MarshalHelper.UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

                operation.Size = sizeof(CardOperation);
                operation.OpCode = (byte)OperationSelector(type);
                operation.POS = terminal; // Номер АЗС или терминала
                operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
                operation.TransactID = transact; // Тут желательно ваш код код транзакции указать
                operation.CardType = (int)CardType.CT_PAYMENT; // Предположим что тип карты неизвестен
                operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
                operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                operation.CardNumber = ""; // Не указывать !!!
                operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                operation.ItemCount = 1; // При аутентификации позиции не заполнять.

                IntPtr pItem1 = Marshal.AllocHGlobal(sizeof(CardOperationItem));
                MarshalHelper.UnMemory<CardOperationItem>.SaveInMem(opItem, ref pItem1);
                IntPtr ppItem1 = Marshal.AllocHGlobal(sizeof(IntPtr));
                MarshalHelper.UnMemory<IntPtr>.SaveInMem(pItem1, ref ppItem1);

                operation.Items = ppItem1;

                MarshalHelper.UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

                if ((res.ErrorCore = OperationSelector(type, obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.E_SUCCESS)
                    throw new Exception("Error Operation No" + res);

                // флаг указывает на то что образ карты был изменен в процессе операции
                // и нужно записать его обратно.
                // Нужно для пополнения карты при просмотре информации
                operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
                if (operation.CardImageChanged != 0)
                {
                    // при ошибке можно попробовать повторить операцию несколько раз 3-10...
                    // если не проходит то это патовая ситуация и для её дальнейшего
                    // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
                    // В теории, сохранённый SerialNumber и CardImage можно попробовать
                    // записать обратно даже при перезапуске ПО, но до следующего
                    // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
                    // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
                    if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.E_SUCCESS)
                        throw new Exception("Error WriteCard No" + res);
                }

                operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
                res.CardInfo = operation.CheckImage;
                return res;
            }
            finally
            {
                MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
                MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                //CardOperation_Free(operationPtr);
            }
        }

        public static OpCode OperationSelector(CardOperationType type)
        {
            switch (type)
            {
                case CardOperationType.Credit:
                    return OpCode.OP_CREDIT;
                case CardOperationType.Debit:
                    return OpCode.OP_DEBIT;
                case CardOperationType.Sale:
                    return OpCode.OP_DEBIT;
                case CardOperationType.Refund:
                    return OpCode.OP_REFUND;
                case CardOperationType.Return:
                    return OpCode.OP_ANNULATE;
                default:
                    return OpCode.OP_AUTHORIZATION;
            }
        }
        public static int OperationSelector(CardOperationType type, IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation)
        {
            switch (type)
            {
                case CardOperationType.Credit:
                    return Credit(objRef, serialNumber, cardImage, operation);
                //case CardOperationType.Debit:
                //    return Debit(objRef, serialNumber, cardImage);
                case CardOperationType.Sale:
                    return Sale(objRef, serialNumber, cardImage, operation);
                //case CardOperationType.Refund:
                //    return Refund(objRef, serialNumber, cardImage/*, operation*/);
                case CardOperationType.Return:
                    return Return(objRef, serialNumber, cardImage, operation);
                default:
                    return -2000;
            }
        }

        //// ВОЗВРАТ ПО КАРТЕ
        //public static ServioCardInfo CardReturn(int transact, CardOperationItem opItem)
        //{
        //    ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
        //    IntPtr obj = new IntPtr();

        //    IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
        //    IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
        //    IntPtr operationPtr = CardOperation_Create();

        //    try
        //    {
        //        if ((res.ErrorCore = Init(_config, ref obj)) != (int)ErrorCodes.E_SUCCESS)
        //        {
        //            if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
        //            {
        //                return res;
        //            }
        //            throw new Exception("Error Init No" + res);
        //        }
        //        MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
        //        if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int)ErrorCodes.E_SUCCESS)
        //        {
        //            if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
        //            {
        //                return res;
        //            }
        //            throw new Exception("Error ReadCard No" + res);
        //        }
        //        var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
        //        SerialnumberT serialNumber = MarshalHelper.UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

        //        operation.Size = sizeof(CardOperation);
        //        operation.OpCode = (byte)OpCode.OP_REFUND;
        //        operation.POS = 1; // Номер АЗС или терминала
        //        operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
        //        operation.TransactID = transact; // Тут желательно ваш код код транзакции указать
        //        operation.CardType = (int)CardType.CT_PAYMENT; // Предположим что тип карты неизвестен
        //        operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
        //        operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
        //        operation.CardNumber = ""; // Не указывать !!!
        //        operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
        //        operation.ItemCount = 1; // При аутентификации позиции не заполнять.

        //        IntPtr pItem1 = Marshal.AllocHGlobal(sizeof(CardOperationItem));
        //        MarshalHelper.UnMemory<CardOperationItem>.SaveInMem(opItem, ref pItem1);
        //        IntPtr ppItem1 = Marshal.AllocHGlobal(sizeof(IntPtr));
        //        MarshalHelper.UnMemory<IntPtr>.SaveInMem(pItem1, ref ppItem1);

        //        operation.Items = ppItem1;

        //        MarshalHelper.UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

        //        if ((res.ErrorCore = Refund(obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.E_SUCCESS)
        //            throw new Exception("Error Return No" + res);

        //        // флаг указывает на то что образ карты был изменен в процессе операции
        //        // и нужно записать его обратно.
        //        // Нужно для пополнения карты при просмотре информации
        //        operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
        //        if (operation.CardImageChanged != 0)
        //        {
        //            // при ошибке можно попробовать повторить операцию несколько раз 3-10...
        //            // если не проходит то это патовая ситуация и для её дальнейшего
        //            // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
        //            // В теории, сохранённый SerialNumber и CardImage можно попробовать
        //            // записать обратно даже при перезапуске ПО, но до следующего
        //            // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
        //            // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
        //            if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.E_SUCCESS)
        //                throw new Exception("Error WriteCard No" + res);
        //        }

        //        operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
        //        res.CardInfo = operation.CheckImage;
        //        return res;
        //    }
        //    finally
        //    {
        //        MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
        //        MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
        //        //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
        //        //CardOperation_Free(operationPtr);
        //    }
        //}
        //// ПОПОЛНЕНИЕ
        //public static ServioCardInfo CardCredit(int transact, CardOperationItem opItem)
        //{
        //    ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
        //    IntPtr obj = new IntPtr();

        //    IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
        //    IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
        //    IntPtr operationPtr = CardOperation_Create();

        //    try
        //    {
        //        if ((res.ErrorCore = Init(_config, ref obj)) != (int)ErrorCodes.E_SUCCESS)
        //        {
        //            if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
        //            {
        //                return res;
        //            }
        //            throw new Exception("Error Init No" + res);
        //        }
        //        MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
        //        if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int)ErrorCodes.E_SUCCESS)
        //        {
        //            if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
        //            {
        //                return res;
        //            }
        //            throw new Exception("Error ReadCard No" + res);
        //        }
        //        var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
        //        SerialnumberT serialNumber = MarshalHelper.UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

        //        operation.Size = sizeof(CardOperation);
        //        operation.OpCode = (byte)OpCode.OP_CREDIT;
        //        operation.POS = 1; // Номер АЗС или терминала
        //        operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
        //        operation.TransactID = transact; // Тут желательно ваш код код транзакции указать
        //        operation.CardType = (int)CardType.CT_PAYMENT; // Предположим что тип карты неизвестен
        //        operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
        //        operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
        //        operation.CardNumber = ""; // Не указывать !!!
        //        operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
        //        operation.ItemCount = 1; // При аутентификации позиции не заполнять.

        //        IntPtr pItem1 = Marshal.AllocHGlobal(sizeof(CardOperationItem));
        //        MarshalHelper.UnMemory<CardOperationItem>.SaveInMem(opItem, ref pItem1);
        //        IntPtr ppItem1 = Marshal.AllocHGlobal(sizeof(IntPtr));
        //        MarshalHelper.UnMemory<IntPtr>.SaveInMem(pItem1, ref ppItem1);

        //        operation.Items = ppItem1;

        //        MarshalHelper.UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

        //        if ((res.ErrorCore = Credit(obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.E_SUCCESS)
        //            throw new Exception("Error Sale No" + res);

        //        // флаг указывает на то что образ карты был изменен в процессе операции
        //        // и нужно записать его обратно.
        //        // Нужно для пополнения карты при просмотре информации
        //        operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
        //        if (operation.CardImageChanged != 0)
        //        {
        //            // при ошибке можно попробовать повторить операцию несколько раз 3-10...
        //            // если не проходит то это патовая ситуация и для её дальнейшего
        //            // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
        //            // В теории, сохранённый SerialNumber и CardImage можно попробовать
        //            // записать обратно даже при перезапуске ПО, но до следующего
        //            // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
        //            // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
        //            if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.E_SUCCESS)
        //                throw new Exception("Error WriteCard No" + res);
        //        }

        //        operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
        //        res.CardInfo = operation.CheckImage;
        //        return res;
        //    }
        //    finally
        //    {
        //        MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
        //        MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
        //        //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
        //        //CardOperation_Free(operationPtr);
        //    }
        //}
        //// СПИСАНИЕ
        //public static ServioCardInfo CardDebit(int transact, CardOperationItem opItem)
        //{
        //    ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
        //    IntPtr obj = new IntPtr();

        //    IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
        //    IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
        //    IntPtr operationPtr = CardOperation_Create();

        //    try
        //    {
        //        if ((res.ErrorCore = Init(_config, ref obj)) != (int)ErrorCodes.E_SUCCESS)
        //        {
        //            if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
        //            {
        //                return res;
        //            }
        //            throw new Exception("Error Init No" + res);
        //        }
        //        MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
        //        if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int)ErrorCodes.E_SUCCESS)
        //        {
        //            if (res.ErrorCore == (int)ErrorCodes.E_CANCEL)
        //            {
        //                return res;
        //            }
        //            throw new Exception("Error ReadCard No" + res);
        //        }
        //        var operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
        //        SerialnumberT serialNumber = MarshalHelper.UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

        //        operation.Size = sizeof(CardOperation);
        //        operation.OpCode = (byte)OpCode.OP_DEBIT;
        //        operation.POS = 1; // Номер АЗС или терминала
        //        operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
        //        operation.TransactID = transact; // Тут желательно ваш код код транзакции указать
        //        operation.CardType = (int)CardType.CT_PAYMENT; // Предположим что тип карты неизвестен
        //        operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
        //        operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
        //        operation.CardNumber = ""; // Не указывать !!!
        //        operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
        //        operation.ItemCount = 1; // При аутентификации позиции не заполнять.

        //        IntPtr pItem1 = Marshal.AllocHGlobal(sizeof(CardOperationItem));
        //        MarshalHelper.UnMemory<CardOperationItem>.SaveInMem(opItem, ref pItem1);
        //        IntPtr ppItem1 = Marshal.AllocHGlobal(sizeof(IntPtr));
        //        MarshalHelper.UnMemory<IntPtr>.SaveInMem(pItem1, ref ppItem1);

        //        operation.Items = ppItem1;

        //        MarshalHelper.UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

        //        if ((res.ErrorCore = Debit(obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.E_SUCCESS)
        //            throw new Exception("Error Sale No" + res);

        //        // флаг указывает на то что образ карты был изменен в процессе операции
        //        // и нужно записать его обратно.
        //        // Нужно для пополнения карты при просмотре информации
        //        operation = MarshalHelper.UnMemory<CardOperation>.ReadInMem(operationPtr);
        //        if (operation.CardImageChanged != 0)
        //        {
        //            // при ошибке можно попробовать повторить операцию несколько раз 3-10...
        //            // если не проходит то это патовая ситуация и для её дальнейшего
        //            // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
        //            // В теории, сохранённый SerialNumber и CardImage можно попробовать
        //            // записать обратно даже при перезапуске ПО, но до следующего
        //            // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
        //            // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
        //            if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.E_SUCCESS)
        //                throw new Exception("Error WriteCard No" + res);
        //        }

        //        operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
        //        res.CardInfo = operation.CheckImage;
        //        return res;
        //    }
        //    finally
        //    {
        //        MarshalHelper.UnMemory.FreeIntPtr(serialNumberPtr);
        //        MarshalHelper.UnMemory.FreeIntPtr(cardImagePtr);
        //        //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
        //        //CardOperation_Free(operationPtr);
        //    }
        //}

    }
}
