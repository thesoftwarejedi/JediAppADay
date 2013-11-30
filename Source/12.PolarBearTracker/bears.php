<markers>
<?php

	$url = "http://www.panda.org/about_wwf/where_we_work/arctic/polar_bear/tracking/index.cfm";

	$file = fopen($url, "r");
	while (!feof($file)) {
		$curLine = fgets($file, 2048);
		if (preg_match('/            <td width="120"><a href="#bear\d\d\d\d">/', $curLine)) {
			$curLine = preg_replace('/            <td width="120"><a href="#bear\d\d\d\d">/', '', $curLine);
			$curLine = preg_replace('/<\/a><\/td>/', '', $curLine);
			$curLine = rtrim($curLine);
			echo '<marker data="';
			echo $curLine;
			echo '" lat="';
			$curLine = fgets($file, 2048);
			$curLine = fgets($file, 2048);
			$curLine = preg_replace('/            <td width="108">/', '', $curLine);
			$curLine = preg_replace('/..<\/td>/', '', $curLine);
			$curLine = rtrim($curLine);
			echo $curLine;
			echo '" lng="';
			$curLine = fgets($file, 2048);
			$curLine = preg_replace('/            <td width="108">/', '', $curLine);
			$curLine = preg_replace('/..<\/td>/', '', $curLine);
			$curLine = rtrim($curLine);
			echo $curLine;
			echo '" />';
		}
	}
	fclose($file);

?>
</markers>