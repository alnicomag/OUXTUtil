using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO : センテンスのパースコードを実装．
// TODO : mbedからイーサネットを使って送られてくるNMEAフォーマットのセンテンスを（取り敢えず）UDPで取得できるようにする．TCPにするかは後で決める．
// TODO : UDPでの取得を非同期で．
// TODO : GPS関連の最上位レイヤにあたるクラスを作成．イベントハンドラによってレイテスト値を通知出来るように．
// TODO : 適宜独自属性を実装？

namespace GpsUtil
{
	/// <summary>
	/// 作者情報を表す属性．
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class AuthorAttribute : Attribute
	{
		public AuthorAttribute(string name) { this.name = name; }
		private readonly string name;
	}

	/// <summary>
	/// 取得できる値の信頼度を表す属性．
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ConfidenceAttribute : Attribute
	{
		public ConfidenceAttribute(bool confidence) { this.confidence = confidence; }
		private readonly bool confidence;
	}

	/// <summary>
	/// GPSモジュールから取得できるNMEAフォーマットのセンテンスを解釈する．
	/// </summary>
	[Author("T.sanada")]
	public class GpsRecord
	{
		public GpsRecord(string gps_sentence)
		{
			original_command = gps_sentence;



		}

		private string original_command;

		public DateTime Time { get; private set; }
		public bool Status { get; private set; }

		/// <summary>
		/// 経度．東経側が正．ddd.ddddddd度表記
		/// </summary>
		public double Longtitude { get; private set; }
		/// <summary>
		/// 緯度．北緯側が正．ddd.ddddddd度表記
		/// </summary>
		public double Latitude { get; private set; }
		/// <summary>
		/// 対地速度[m/s]
		/// </summary>
		[Confidence(false)]
		public double Velocity { get; set; }
		/// <summary>
		/// 進行方向．真北に対しての度．
		/// </summary>
		[Confidence(false)]
		public double Direction { get; set; }
		/// <summary>
		/// 地磁気の偏角．東側への偏角が正．
		/// </summary>
		public double Declination { get; set; }

		public PositioningQuality Positioning { get; set; }

		/// <summary>
		/// ビュー内の総衛星数
		/// </summary>
		public int TotalSatellite { get { return satellites.Count; } }

		/// <summary>
		/// 位置精度
		/// </summary>
		public double PositionalPrecision { get; set; }

		/// <summary>
		/// 標高[m]．
		/// </summary>
		[Confidence(false)]
		public double Altitude { get; set; }

		/// <summary>
		/// ジオイド高さ[m]．
		/// </summary>
		public double GeoidHeight { get; set; }

		private List<Satellite> satellites;
	}

	/// <summary>
	/// GPS用NMEA talker+message
	/// </summary>
	public enum GpsModuleCommand : int
	{
		/// <summary>
		/// Position Response Message
		/// GPS位置情報
		/// </summary>
		GPGGA,
		/// <summary>
		/// Satellite Used Response Message
		/// 使用している衛星とDOP値
		/// </summary>
		GPGSA,
		/// <summary>
		/// Recommended Minimum Course Response Message
		/// 最小構成の航法情報
		/// </summary>
		GPRMC,
		/// <summary>
		/// Satellites-in-View Response Message
		/// 利用可能な衛星情報
		/// </summary>
		GPGSV,
		/// <summary>
		/// Latitude/Longitude Response Message
		/// 緯度，及び経度情報
		/// </summary>
		GPGLL,
		/// <summary>
		/// Course over Ground and Ground Speed Response Message
		/// 進路，及び速度情報
		/// </summary>
		GPVTG,
		/// <summary>
		/// Time and Date Response Message
		/// </summary>
		GPZDA,
		/// <summary>
		/// GPS Almanac Response Message
		/// </summary>
		GPALM,

		#region GPS用NMEAセンテンスコードの全てをメモ
		/*
$GPAAM - Waypoint Arrival Alarm
$GPALM - GPS Almanac Data
$GPAPA - Autopilot Sentence "A"
$GPAPB - Autopilot Sentence "B"
$GPASD - Autopilot System Data
$GPBEC - Bearing & Distance to Waypoint, Dead Reckoning
$GPBOD - Bearing, Origin to Destination
$GPBWC - Bearing & Distance to Waypoint, Great Circle
$GPBWR - Bearing & Distance to Waypoint, Rhumb Line
$GPBWW - Bearing, Waypoint to Waypoint
$GPDBT - Depth Below Transducer
$GPDCN - Decca Position
$GPDPT - Depth
$GPFSI - Frequency Set Information
$GPGGA - Global Positioning System Fix Data
$GPGLC - Geographic Position, Loran-C
$GPGLL - Geographic Position, Latitude/Longitude
$GPGSA - GPS DOP and Active Satellites
$GPGSV - GPS Satellites in View
$GPGXA - TRANSIT Position
$GPHDG - Heading, Deviation & Variation
$GPHDT - Heading, True
$GPHSC - Heading Steering Command
$GPLCD - Loran-C Signal Data
$GPMTA - Air Temperature (to be phased out)
$GPMTW - Water Temperature
$GPMWD - Wind Direction
$GPMWV - Wind Speed and Angle
$GPOLN - Omega Lane Numbers
$GPOSD - Own Ship Data
$GPR00 - Waypoint active route (not standard)
$GPRMA - Recommended Minimum Specific Loran-C Data
$GPRMB - Recommended Minimum Navigation Information
$GPRMC - Recommended Minimum Specific GPS/TRANSIT Data
$GPROT - Rate of Turn
$GPRPM - Revolutions
$GPRSA - Rudder Sensor Angle
$GPRSD - RADAR System Data
$GPRTE - Routes
$GPSFI - Scanning Frequency Information
$GPSTN - Multiple Data ID
$GPTRF - Transit Fix Data
$GPTTM - Tracked Target Message
$GPVBW - Dual Ground/Water Speed
$GPVDR - Set and Drift
$GPVHW - Water Speed and Heading
$GPVLW - Distance Traveled through the Water
$GPVPW - Speed, Measured Parallel to Wind
$GPVTG - Track Made Good and Ground Speed
$GPWCV - Waypoint Closure Velocity
$GPWNC - Distance, Waypoint to Waypoint
$GPWPL - Waypoint Location
$GPXDR - Transducer Measurements
$GPXTE - Cross-Track Error, Measured
$GPXTR - Cross-Track Error, Dead Reckoning
$GPZDA - Time & Date
$GPZFO - UTC & Time from Origin Waypoint
$GPZTG - UTC & Time to Destination Waypoint
*/
		#endregion
	}

	/// <summary>
	/// ステータス．@RMCセンテンス
	/// </summary>
	enum Status
	{
		Ok,
		Invalid,
	}

	/// <summary>
	/// 位置特定品質．@GGAセンテンス
	/// </summary>
	enum PositioningQuality : int
	{
		/// <summary>
		/// 位置特定できない
		/// </summary>
		Invalid = 0,
		/// <summary>
		/// Standard Positioning Service（標準測位サービス）
		/// </summary>
		Standard = 1,
		/// <summary>
		/// Differential GPS
		/// </summary>
		Differential = 2
	}

	/// <summary>
	/// 方位
	/// </summary>
	enum CompassPoint
	{
		North,
		South,
		East,
		West
	}

	/// <summary>
	/// 測位モード．@GSAセンテンス
	/// </summary>
	enum PositioningMode
	{
		/// <summary>
		/// ２次元測位固定モード
		/// </summary>
		Manual,
		/// <summary>
		/// ２次元測位／３次元測位自動切り替えモード
		/// </summary>
		Auto
	}

	/// <summary>
	/// 測位状態．@GSAセンテンス
	/// </summary>
	enum PositioningState
	{
		/// <summary>
		/// 未測位
		/// </summary>
		Invalid,
		/// <summary>
		/// 2次元測位中
		/// </summary>
		D2,
		/// <summary>
		/// 3次元測位中
		/// </summary>
		D3
	}

	enum Mode
	{
		/// <summary>
		/// 無効
		/// </summary>
		Invalid,
		/// <summary>
		/// 自律方式
		/// </summary>
		Autonomous,
		/// <summary>
		/// 干渉測位方式
		/// </summary>
		Differential,
		/// <summary>
		/// 推定
		/// </summary>
		Estimated
	}
	
	/// <summary>
	/// @GSVセンテンス
	/// </summary>
	struct Satellite
	{
		public Satellite(int prnNumber, int elevation, int azimuth, int snrate)
		{
			PrnNumber = prnNumber;
			Elevation = elevation;
			Azimuth = azimuth;
			SnRate = snrate;
		}
		/// <summary>
		/// 衛星番号
		/// </summary>
		public int PrnNumber;
		/// <summary>
		/// 衛星仰角
		/// </summary>
		public int Elevation;
		/// <summary>
		/// 衛星方位
		/// </summary>
		public int Azimuth;
		/// <summary>
		/// 受信レベル(SN比 db)
		/// </summary>
		public int SnRate;
	}

	/// <summary>
	/// 拡張メソッドの定義用静的クラス
	/// </summary>
	public static class GpsUtilExtensions
	{
		public static string ToStringExt(this GpsModuleCommand e)
		{
			switch (e)
			{
				case GpsModuleCommand.GPGGA: return "GPGGA";

				default: throw new ArgumentOutOfRangeException();
			}
		}

		public static string ToStringExt(this CompassPoint e)
		{
			switch (e)
			{
				case CompassPoint.North: return "N";
				case CompassPoint.South: return "S";
				case CompassPoint.East: return "E";
				case CompassPoint.West: return "W";
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
}
