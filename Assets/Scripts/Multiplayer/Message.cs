using System;
using UnityEngine;

namespace Multiplayer
{
	public class Message
	{
		public int code;
		private string data;
		private string[] split; 

		public Message () {
			code = 0;
			data = "";
		}

		public Message (int code) {
			this.code = code;
			data = "";
		}

		public Message (int code, string data) {
			this.code = code;
			this.data = data;
		}

		public Message (byte[] data) {
			string[] s = System.Text.ASCIIEncoding.ASCII.GetString (data).Split ('$');
			this.code = int.Parse (s [0]);
			this.data = s [1];
			split = this.data.Split ('\n');
		}

		public void Add (Vector2 vec) {
			data += string.Format ("{0}|{1}\n", vec.x, vec.y);
		}

		public void Add (Vector3 vec) {
			data += string.Format ("{0}|{1}|{2}\n", vec.x, vec.y, vec.z);
		}

		public void Add (Quaternion qua) {
			data += string.Format ("{0}|{1}|{2}|{3}\n", qua.x, qua.y, qua.z, qua.w);
		}

		public void Add (bool b) {
			data += b ? "1\n" : "0\n";
		}

		public void Add (int i) {
			data += i.ToString () + "\n";
		}

		public Vector2 GetVector2 (int index) {
			return DecodeVector2 (split [index]);
		}

		public Vector3 GetVector3 (int index) {
			return DecodeVector3 (split [index]);
		}

		public Quaternion GetQuaternion (int index) {
			return DecodeQuaternion (split [index]);
		}

		public bool GetBool (int index) {
			return DecodeBool (split [index]);
		}

		public int GetInt (int index) {
			return DecodeInt (split [index]);
		}

		public byte[] GetBytes () {
			return System.Text.ASCIIEncoding.ASCII.GetBytes (code.ToString() + "$" + data);
		}

		private static Vector2 DecodeVector2 (string data) {
			string[] s = data.Split ('|');
			return new Vector2 (float.Parse (s [0]), float.Parse (s [1]));
		}

		private static Vector3 DecodeVector3 (string data) {
			string[] s = data.Split ('|');
			return new Vector3 (float.Parse (s [0]), float.Parse (s [1]), float.Parse (s [2]));
		}

		private static Quaternion DecodeQuaternion (string data) {
			string[] s = data.Split ('|');
			return new Quaternion (float.Parse (s [0]), float.Parse (s [1]), float.Parse (s [2]), float.Parse (s [3]));
		}

		private static bool DecodeBool (string data) {
			return data [0] == '1';
		}

		private static int DecodeInt (string data) {
			return int.Parse (data);
		}
	}
}

