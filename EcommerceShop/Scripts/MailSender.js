$(document).ready(function () {
    $("#emailInput").on("input", function () {
        if ($(this).val() !== "") {
            $("#otpInputContainer").show();
            $("#sendOTPButton").prop('disabled', false);
        } else {
            $("#otpInputContainer").hide();
            $("#sendOTPButton").prop('disabled', true);
            $("#signupButton").prop('disabled', true);
        }
    });

    $("#sendOTPButton").click(function () {
        var email = $("#emailInput").val();

        $.ajax({
            url: '/Home/GenerateOTP',
            type: 'POST',
            data: { email: email },
            success: function (response) {
                alert("OTP sent successfully. Check your email.");
                $("#signupButton").prop('disabled', false);

                // Make OTP field editable
                $("#otpInputContainer input[name='otp']").prop('readonly', false).focus();
            },
            error: function (error) {
                alert("Error sending OTP. Please try again later.");
            }
        });
    });

    // Fade out error message after a few seconds
    $("#errorMsg").delay(3000).fadeOut('slow');
});