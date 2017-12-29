using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management;
using System.ServiceProcess;
using System.Timers;
using System.Web;
using System.Web.Services;

namespace LimitServiceUsingWeb
{
    /// <summary>
    /// LimitService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class LimitService : System.Web.Services.WebService
    {
        /// <summary>
        /// 主板序列号
        /// </summary>
        string destBoardSerialNumber = "BSN12345678901234567";

        /// <summary>
        /// cpu序列号
        /// </summary>
        string destCpuSerialNumber = "BFEBFBFF000006FD";

        /// <summary>
        /// 指定日期（超过该日期视为过期）
        /// </summary>
        DateTime destDatetime = new DateTime(2016, 6, 30);

        /// <summary>
        /// 计时器（检测是否过期）
        /// </summary>
        Timer timer = null;

        [WebMethod]
        public void CheckCanUsing()
        {
            bool isdestComputer = CheckIsDestSeriral(this.destBoardSerialNumber, this.destCpuSerialNumber);
            //不为制定的设备,进行处理
            if (!isdestComputer)
            {
                this.StopIIS();
                return;
            }
            if (timer == null)
            {
                timer = new Timer();
                timer.Elapsed += timer_Elapsed;
                timer.Interval =  2000;
                timer.Start();
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool isOverdue = this.IsOverdue(this.destDatetime);
            if (isOverdue)
            {
                this.StopIIS();
            }
        }

        #region 查看是否已过期

        public bool IsOverdue(DateTime destDatetime)
        {
            bool result = false;

            if (DateTime.Now > destDatetime)
            {
                result = true;
            }
            return result;
        }

        #endregion

        #region 查看cpu和主板序列号是否一致

        public bool CheckIsDestSeriral(string destBoardSerialNumber, string destCpuSerialNumber)
        {
            bool isdestComputer = false;
            //获取本地主板序列号
            string boardSearialNumber = GetBaseBoardSerialNumber();
            //获取本地cpu序列号
            string cpuSearialNumber = GetCPUSerialNumber();
            //是否为制定的设备（服务器）

            if (boardSearialNumber.Equals(destBoardSerialNumber))
            {
                isdestComputer = true;
            }
            if (destBoardSerialNumber.Equals(destCpuSerialNumber))
            {
                isdestComputer = true;
            }
            return isdestComputer;
        }

        #endregion

        #region 获取cpu序列号和主板序列号

        /// <summary>
        /// 获取主板编号
        /// </summary>
        /// <returns>反回主板编号字符串</returns>
        public static string GetBaseBoardSerialNumber()
        {
            string basebrardSerialNumber = string.Empty;
            ManagementClass mc = new ManagementClass("WIN32_BaseBoard");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                basebrardSerialNumber = mo["SerialNumber"].ToString();
                break;
            }
            mc.Dispose();
            moc.Dispose();
            return basebrardSerialNumber;
        }

        /// <summary>
        /// 过去CPU序列号
        /// </summary>
        /// <returns>反回序列号字符串</returns>
        public static string GetCPUSerialNumber()
        {
            string cpuSerialNumber = string.Empty;
            ManagementClass mc = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuSerialNumber = mo["ProcessorId"].ToString();
                break;
            }
            mc.Dispose();
            moc.Dispose();
            return cpuSerialNumber;
        }

        #endregion

        #region 关闭iis

        public void StopIIS()
        {

            ServiceController sc = new ServiceController("W3SVC");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
            }
        }

        #endregion
    }
}