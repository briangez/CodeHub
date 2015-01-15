
using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace CodeHub.iOS.DialogElements
{
	public interface IColorizeBackground {
		void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath);
	}
	

	public class EntryElement : Element {
		/// <summary>
		///   The value of the EntryElement
		/// </summary>
		public string Value { 
			get {
				if (entry == null)
					return val;
				var newValue = entry.Text;
				if (newValue == val)
					return val;
				val = newValue;

				if (Changed != null)
					Changed (this, EventArgs.Empty);
				return val;
			}
			set {
				val = value;
				if (entry != null)
					entry.Text = value;
			}
		}
		protected string val;

		public UIKeyboardType KeyboardType {
			get {
				return keyboardType;
			}
			set {
				keyboardType = value;
				if (entry != null)
					entry.KeyboardType = value;
			}
		}
		
		/// <summary>
		/// The type of Return Key that is displayed on the
		/// keyboard, you can change this to use this for
		/// Done, Return, Save, etc. keys on the keyboard
		/// </summary>
		public UIReturnKeyType? ReturnKeyType {
			get {
				return returnKeyType;
			}
			set {
				returnKeyType = value;
				if (entry != null && returnKeyType.HasValue)
					entry.ReturnKeyType = returnKeyType.Value;
			}
		}
		
		public UITextAutocapitalizationType AutocapitalizationType {
			get {
				return autocapitalizationType;	
			}
			set { 
				autocapitalizationType = value;
				if (entry != null)
					entry.AutocapitalizationType = value;
			}
		}
		
		public UITextAutocorrectionType AutocorrectionType { 
			get { 
				return autocorrectionType;
			}
			set { 
				autocorrectionType = value;
				if (entry != null)
					this.autocorrectionType = value;
			}
		}
		
		public UITextFieldViewMode ClearButtonMode { 
			get { 
				return clearButtonMode;
			}
			set { 
				clearButtonMode = value;
				if (entry != null)
					entry.ClearButtonMode = value;
			}
		}

		public UITextAlignment TextAlignment {
			get {
				return textalignment;
			}
			set{
				textalignment = value;
				if (entry != null) {
					entry.TextAlignment = textalignment;
				}
			}
		}
		UITextAlignment textalignment = UITextAlignment.Left;
		UIKeyboardType keyboardType = UIKeyboardType.Default;
		UIReturnKeyType? returnKeyType = null;
		UITextAutocapitalizationType autocapitalizationType = UITextAutocapitalizationType.Sentences;
		UITextAutocorrectionType autocorrectionType = UITextAutocorrectionType.Default;
		UITextFieldViewMode clearButtonMode = UITextFieldViewMode.Never;
		bool isPassword, becomeResponder;
		UITextField entry;
		string placeholder;

		public event EventHandler Changed;
		public event Func<bool> ShouldReturn;
		public EventHandler EntryStarted {get;set;}
		public EventHandler EntryEnded {get;set;}
        public UIFont TitleFont { get; set; }
        public UIFont EntryFont { get; set; }
        public UIColor TitleColor { get; set; }

		public EntryElement (string caption, string placeholder, string value)
		{ 
            TitleFont = UIFont.BoldSystemFontOfSize (17);
            EntryFont = UIFont.SystemFontOfSize(17);
            TitleColor = UIColor.Black;
			Value = value;
            Caption = caption;
			this.placeholder = placeholder;
		}
		
		public EntryElement (string caption, string placeholder, string value, bool isPassword)
		{
            TitleFont = UIFont.BoldSystemFontOfSize (17);
            EntryFont = UIFont.SystemFontOfSize(17);
            TitleColor = UIColor.Black;
			Value = value;
            Caption = caption;
			this.isPassword = isPassword;
			this.placeholder = placeholder;
		}

		// 
		// Computes the X position for the entry by aligning all the entries in the Section
		//
		CGSize ComputeEntryPosition (UITableView tv, UITableViewCell cell)
		{
			if (Section.EntryAlignment.Width != 0)
                return Section.EntryAlignment;
			
			// If all EntryElements have a null Caption, align UITextField with the Caption
			// offset of normal cells (at 10px).
            var max = new CGSize (-15, UIStringDrawing.StringSize ("M", TitleFont).Height);
            foreach (var e in Section){
				var ee = e as EntryElement;
				if (ee == null)
					continue;
				
				if (ee.Caption != null) {
                    var size = UIStringDrawing.StringSize (ee.Caption, TitleFont);
					if (size.Width > max.Width)
						max = size;
				}
			}

            Section.EntryAlignment = new CGSize (25f + (nfloat)Math.Min (max.Width, 160), max.Height);
            return Section.EntryAlignment;
		}

		protected virtual UITextField CreateTextField (CGRect frame)
		{
			return new UITextField (frame) {
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
				Placeholder = placeholder ?? "",
				SecureTextEntry = isPassword,
				Text = Value ?? "",
				Tag = 1,
				TextAlignment = textalignment,
				ClearButtonMode = ClearButtonMode,
                Font = EntryFont,
			};
		}
		
		static NSString cellkey = new NSString ("EntryElement");

		UITableViewCell cell;
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (cell == null) {
                cell = new UITableViewCell (UITableViewCellStyle.Default, cellkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			} 
			cell.TextLabel.Text = Caption;

			var offset = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) ? 20 : 90;
			cell.Frame = new CGRect(cell.Frame.X, cell.Frame.Y, tv.Frame.Width-offset, cell.Frame.Height);
			CGSize size = ComputeEntryPosition (tv, cell);
			var yOffset = (cell.ContentView.Bounds.Height - size.Height) / 2 - 1;
			var width = cell.ContentView.Bounds.Width - size.Width;
			if (textalignment == UITextAlignment.Right) {
				// Add padding if right aligned
				width -= 10;
			}
            var entryFrame = new CGRect (size.Width, yOffset + 2f, width, size.Height);

			if (entry == null) {
				entry = CreateTextField (entryFrame);
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {                                        
					FetchValue ();
					if (EntryEnded != null) {
						EntryEnded (this, null);
					}
				};
				entry.ShouldReturn += delegate {

					if (ShouldReturn != null)
						return ShouldReturn ();

                    RootElement root = GetRootElement();
					EntryElement focus = null;

					if (root == null)
						return true;

					foreach (var s in root) {
						foreach (var e in s) {
							if (e == this) {
								focus = this;
							} else if (focus != null && e is EntryElement) {
								focus = e as EntryElement;
								break;
							}
						}

						if (focus != null && focus != this)
							break;
					}

					if (focus != this)
						focus.BecomeFirstResponder (true);
					else 
						focus.ResignFirstResponder (true);

					return true;
				};
				entry.Started += delegate {
					EntryElement self = null;

					if (EntryStarted != null) {
						EntryStarted (this, null);
					}

					if (!returnKeyType.HasValue) {
						var returnType = UIReturnKeyType.Default;

                        foreach (var e in Section) {
							if (e == this)
								self = this;
							else if (self != null && e is EntryElement)
								returnType = UIReturnKeyType.Next;
						}
						entry.ReturnKeyType = returnType;
					} else
						entry.ReturnKeyType = returnKeyType.Value;

					tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, true);
				};
				cell.ContentView.AddSubview (entry);
			} else
				entry.Frame = entryFrame;

			if (becomeResponder){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
			entry.KeyboardType = KeyboardType;

			entry.AutocapitalizationType = AutocapitalizationType;
			entry.AutocorrectionType = AutocorrectionType;
			cell.TextLabel.Text = Caption;
            cell.TextLabel.Font = TitleFont;
            cell.TextLabel.TextColor = TitleColor;

			return cell;
		}
		
		/// <summary>
		///  Copies the value from the UITextField in the EntryElement to the
		//   Value property and raises the Changed event if necessary.
		/// </summary>
		public void FetchValue ()
		{
			if (entry == null)
				return;

			var newValue = entry.Text;
			if (newValue == Value)
				return;
			
			Value = newValue;
			
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}


		public override void Selected (UITableView tableView, NSIndexPath indexPath)
		{
			BecomeFirstResponder(true);
            base.Selected(tableView, indexPath);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null && Value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1) || base.Matches (text);
		}
		
		/// <summary>
		/// Makes this cell the first responder (get the focus)
		/// </summary>
		/// <param name="animated">
		/// Whether scrolling to the location of this cell should be animated
		/// </param>
		public virtual void BecomeFirstResponder (bool animated)
		{
			becomeResponder = true;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
		}

		public virtual void ResignFirstResponder (bool animated)
		{
			becomeResponder = false;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null)
				entry.ResignFirstResponder ();
		}
	}
}
