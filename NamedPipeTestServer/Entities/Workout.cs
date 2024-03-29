﻿using System;
using System.Collections.Generic;

namespace ExorLive.Client.Entities
{
	public class Workout
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DateTime LastChangedAt { get; set; }
		public int Owner { get; set; }
		public int Executor { get; set; }
		public int ExerciseCount { get; set; }
		public List<Exercise> Exercises { get; set; }
	}

	public class Exercise
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<List<ExerciseData>> Data {get; set;}
	}

	public class ExerciseData
	{
		public string Key { get; set; }
		public string Value { get; set; }
		public string Unit { get; set; }
	}

	public class Activity
	{
		public int Id { get; set; }
		public int Name { get; set; }
		public int Executor { get; set; }
		public DateTime At { get; set; }
		public bool Executed { get; set; }
		public Workout Workout { get; set; }
	}

}
