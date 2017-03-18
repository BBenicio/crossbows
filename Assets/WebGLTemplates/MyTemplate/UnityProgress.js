function UnityProgress (dom) {
	this.progress = 0;
	this.message = "";

	this.parent = document.getElementById("loading");
	this.bar = document.querySelector(".frame .bar");
	this.progressText = document.querySelector(".frame .bar .progress");
	this.messageText = document.querySelector(".message");

	this.SetProgress = function (progress) {
		console.log("UnityProgress.SetProgress() " + progress);
		this.progress = progress;

		this.Update ();
	}

	this.SetMessage = function (message) {
		console.log("UnityProgress.SetMessage() " + message);
		if (message.indexOf(".") != -1) {
			this.message = message.substr(0, message.indexOf("."));
		}
		if (this.message.indexOf("(") != -1) {
			this.message = this.message.substr(0, this.message.indexOf("("));
		}

		this.message = this.message.trim();

		this.Update ();
	}

	this.Clear = function() {
		document.body.removeChild(this.parent);
	}

	this.Update = function() {
		this.bar.style.width = (this.progress * 100) + "%";
		this.progressText.textContent = Math.round(this.progress * 100) + "%";
		this.messageText.textContent = this.message;
	}

	this.Update ();
}