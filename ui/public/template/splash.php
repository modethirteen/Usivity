<div class="splash">
	<div class="accounts">
		<ul class="tabs">
			<li alt=".newaccount">New User</li>
			<li alt=".existingaccount">Existing User</li>
		</ul>
		<div class="newaccount tabtarget account">
			<form class="message_send" id="{@id}" action="{@href}">	
				<span class="ititle">email address</span>
				<input name="email" type="text"/>
				<span class="ititle">company name</span>
				<input name="email" type="company"/>
				<div class="buttons">
					<button type="submit" class="button apply">register new account</button>
				</div>
			</form>
		</div>
		<div class="existingaccount tabtarget account">
			<form class="message_send" id="{@id}" action="{@href}">	
				<span class="ititle">email address</span>
				<input name="username" type="text"/>
				
				<span class="ititle">password</span>
				<input name="password" type="password"/>
				
				<div class="buttons">
					<button type="submit" class="button apply">login</button>
				</div>
			</form>
		</div>
	</div>
</div>