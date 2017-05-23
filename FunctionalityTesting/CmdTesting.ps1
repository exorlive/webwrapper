#
# Script.ps1
#
$bin = (ps -Name "ExorLive.Client.WebWrapper").Path;
if(!$bin) {
	echo "Webwrapper is not running."
	exit;
}

function get-argumentname {
	Write-Host "Select a test"
	Write-Host "1: Query exercises"
	Write-Host "2: Query workouts"
	Write-Host "3: Open workout"
	Write-Host "4: Login with single sign-on"
	Write-Host "5: Select or create a contact"
	Write-Host "6: Go to tab"
	$input = Read-Host "?"
	switch($input) {
		'1' { 
			$value = Read-Host "Input a exercise title";
			return "queryexercises=$value";
		}
		'2' { 
			$value = Read-Host "Input a workout title";
			return "queryworkouts=$value";
		}
		'3' { 
			$value = Read-Host "Input a Workout id";
			return "openworkout=$value";
		}
		'4' {
			$value = Read-Host "Input your username";
			return "signon=$value";
		}
		'5' {
			return get-contact;
		}
		'6' {
			$value = get-tab;
			return "tab=$value";
		}
	}
	echo "Invalid selection."
	exit;
}

function get-tab {
	Write-Host "1: Tab Home"
	Write-Host "2: Tab Workouts"
	Write-Host "3: Tab Plan"
	Write-Host "4: Tab Stats"
	Write-Host "5: Tab Contacts"
	Write-Host "6: Tab Admin"
	$value = Read-Host "?";
	switch($input) {
		'1' { return 'Home' }
		'2' { return 'Workouts' }
		'3' { return 'Plan' }
		'4' { return 'Statistics' }
		'5' { return 'Contacts' }
		'6' { return 'Admin' }
	}
	return 'Home'
}

function get-contact {
	$userid = Read-Host "Input customId/clientId"
	$firstname = Read-Host "Input firstname"
	$lastname = Read-Host "Input lastname"
	$email = Read-Host "Input email"
	$dob = Read-Host "Input Date of Birth (Dateformat YYYY-MM-DD prefered)"
	$address = Read-Host "Input address"
	$phonehome = Read-Host "Input phone(home)"
	$phonework = Read-Host "Input phone(work)"
	$mobile = Read-Host "Input phone(mobile)"
	$country = Read-Host "Input country"
	$zipcode = Read-Host "Input zipcode"
	$location = Read-Host "Input location"
	return @(
		"id=$userid", 
		"firsname=$firstname", 
		"lastname=$lastname", 
		"email=$email", 
		"dateofbirth=$dob",
		"address=$address",
		"phonehome=$phonehome",
		"phonework=$phonework",
		"mobile=$mobile",
		"country=$country",
		"zipcode=$zipcode",
		"location=$location"
	)
}

$arguments = @()
$done = $false;
while($done -eq $false) {
	$arguments += get-argumentname;

	$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Add more arguments."
	$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "I'm done, run the script."
	$options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
	$result = $host.ui.PromptForChoice("Add more choices?", "Do you want to add more options?", $options, 1)
	switch ($result)
    {
        0 {}
        1 { $done = $true; }
    }
}

echo "Running executable: $bin $arguments";
Start-Process -FilePath $bin -ArgumentList $arguments
