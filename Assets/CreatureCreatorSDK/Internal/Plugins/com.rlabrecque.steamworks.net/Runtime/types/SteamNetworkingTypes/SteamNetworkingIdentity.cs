// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
	
#endif

#if !DISABLESTEAMWORKS

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks
{
	/// An abstract way to represent the identity of a network host.  All identities can
	/// be represented as simple string.  Furthermore, this string representation is actually
	/// used on the wire in several places, even though it is less efficient, in order to
	/// facilitate forward compatibility.  (Old client code can handle an identity type that
	/// it doesn't understand.)
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SteamNetworkingIdentity : System.IEquatable<SteamNetworkingIdentity>
	{
		/// Type of identity.
		public ESteamNetworkingIdentityType m_eType;

		//
		// Internal representation.  Don't access this directly, use the accessors!
		//
		// Number of bytes that are relevant below.  This MUST ALWAYS be
		// set.  (Use the accessors!)  This is important to enable old code to work
		// with new identity types.
		private int m_cbSize;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		private uint[] m_reserved; // Pad structure to leave easy room for future expansion

		// Max sizes
		public const int k_cchMaxString = 128; // Max length of the buffer needed to hold any identity, formatted in string format by ToString
		public const int k_cchMaxGenericString = 32; // Max length of the string for generic string identities.  Including terminating '\0'
		public const int k_cchMaxXboxPairwiseID = 33; // Including terminating '\0'
		public const int k_cbMaxGenericBytes = 32;

		//
		// Get/Set in various formats.
		//

		public void Clear() {
			NativeMethods.SteamAPI_SteamNetworkingIdentity_Clear(ref this);
		}

		// Return true if we are the invalid type.  Does not make any other validity checks (e.g. is SteamID actually valid)
		public bool IsInvalid() {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_IsInvalid(ref this);
		}

		public void SetSteamID(CSteamID steamID) {
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetSteamID(ref this, (ulong)steamID);
		}

		// Return black CSteamID (!IsValid()) if identity is not a SteamID
		public CSteamID GetSteamID() {
			return (CSteamID)NativeMethods.SteamAPI_SteamNetworkingIdentity_GetSteamID(ref this);
		}

		// Takes SteamID as raw 64-bit number
		public void SetSteamID64(ulong steamID) {
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetSteamID64(ref this, steamID);
		}

		// Returns 0 if identity is not SteamID
		public ulong GetSteamID64() {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetSteamID64(ref this);
		}

		// Returns false if invalid length
		public bool SetXboxPairwiseID(string pszString)
		{
			using (var pszString2 = new InteropHelp.UTF8StringHandle(pszString)) {
				return NativeMethods.SteamAPI_SteamNetworkingIdentity_SetXboxPairwiseID(ref this, pszString2);
			}
		}

		// Returns nullptr if not Xbox ID
		public string GetXboxPairwiseID()
		{
			return InteropHelp.PtrToStringUTF8(NativeMethods.SteamAPI_SteamNetworkingIdentity_GetXboxPairwiseID(ref this));
		}

		public void SetPSNID(ulong id)
		{
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetPSNID(ref this, id);
		}

		// Returns 0 if not PSN
		public ulong GetPSNID()
		{
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetPSNID(ref this);
		}

		public void SetStadiaID(ulong id)
		{
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetStadiaID(ref this, id);
		}

		// Returns 0 if not Stadia
		public ulong GetStadiaID()
		{
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetStadiaID(ref this);
		}

		// Set to specified IP:port
		public void SetIPAddr(SteamNetworkingIPAddr addr) {
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetIPAddr(ref this, ref addr);
		}

		// returns null if we are not an IP address.
		public SteamNetworkingIPAddr GetIPAddr(){
			throw new System.NotImplementedException();
			// TODO: Should SteamNetworkingIPAddr be a class?
			//       or should this return some kind of pointer instead?
			//return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetIPAddr(ref this);
		}

		public void SetIPv4Addr(uint nIPv4, ushort nPort) {
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetIPv4Addr(ref this, nIPv4, nPort);
		}

		// returns 0 if we are not an IPv4 address.
		public uint GetIPv4() {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetIPv4(ref this);
		}

		public ESteamNetworkingFakeIPType GetFakeIPType() {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetFakeIPType(ref this);
		}

		public bool IsFakeIP() {
			return GetFakeIPType() > ESteamNetworkingFakeIPType.k_ESteamNetworkingFakeIPType_NotFake;
		}

		// "localhost" is equivalent for many purposes to "anonymous."  Our remote
		// will identify us by the network address we use.
		// Set to localhost.  (We always use IPv6 ::1 for this, not 127.0.0.1)
		public void SetLocalHost() {
			NativeMethods.SteamAPI_SteamNetworkingIdentity_SetLocalHost(ref this);
		}

		// Return true if this identity is localhost.
		public bool IsLocalHost() {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_IsLocalHost(ref this);
		}

		// Returns false if invalid length
		public bool SetGenericString(string pszString) {
			using (var pszString2 = new InteropHelp.UTF8StringHandle(pszString)) {
				return NativeMethods.SteamAPI_SteamNetworkingIdentity_SetGenericString(ref this, pszString2);
			}
		}

		// Returns nullptr if not generic string type
		public string GetGenericString() {
			return InteropHelp.PtrToStringUTF8(NativeMethods.SteamAPI_SteamNetworkingIdentity_GetGenericString(ref this));
		}

		// Returns false if invalid size.
		public bool SetGenericBytes(byte[] data, uint cbLen) {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_SetGenericBytes(ref this, data, cbLen);
		}

		// Returns null if not generic bytes type
		public byte[] GetGenericBytes(out int cbLen) {
			throw new System.NotImplementedException();
			//return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetGenericBytes(ref this, out cbLen);
		}

		/// See if two identities are identical
		public bool Equals(SteamNetworkingIdentity x) {
			return NativeMethods.SteamAPI_SteamNetworkingIdentity_IsEqualTo(ref this, ref x);
		}

		/// Print to a human-readable string.  This is suitable for debug messages
		/// or any other time you need to encode the identity as a string.  It has a
		/// URL-like format (type:<type-data>).  Your buffer should be at least
		/// k_cchMaxString bytes big to avoid truncation.
		///
		/// See also SteamNetworkingIPAddrRender
		public void ToString(out string buf) {
			IntPtr buf2 = Marshal.AllocHGlobal(k_cchMaxString);
			NativeMethods.SteamAPI_SteamNetworkingIdentity_ToString(ref this, buf2, k_cchMaxString);
			buf = InteropHelp.PtrToStringUTF8(buf2);
			Marshal.FreeHGlobal(buf2);
		}

		/// Parse back a string that was generated using ToString.  If we don't understand the
		/// string, but it looks "reasonable" (it matches the pattern type:<type-data> and doesn't
		/// have any funky characters, etc), then we will return true, and the type is set to
		/// k_ESteamNetworkingIdentityType_UnknownType.  false will only be returned if the string
		/// looks invalid.
		public bool ParseString(string pszStr) {
			using (var pszStr2 = new InteropHelp.UTF8StringHandle(pszStr)) {
				return NativeMethods.SteamAPI_SteamNetworkingIdentity_ParseString(ref this, pszStr2);
			}
		}
	}
}

#endif // !DISABLESTEAMWORKS
