﻿/*
 * Copyright (c) 2014-2015 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 * 
 * http://github.com/piranhacms/piranha.vnext
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Piranha.Models
{
	/// <summary>
	/// Posts are used to create content not positioned in the
	/// site structure.
	/// </summary>
	public sealed class Post : Base.Content<PostType>, Data.IModel, Data.IChanges, Data.IPublishable
	{
		#region Properties
		/// <summary>
		/// Gets/sets the id of the content type.
		/// </summary>
		public override Guid TypeId { get; set; }

		/// <summary>
		/// Gets/sets the unique slug.
		/// </summary>
		public override string Slug { get; set; }

		/// <summary>
		/// Gets/sets the optional excerpt.
		/// </summary>
		public string Excerpt { get; set; }

		/// <summary>
		/// Gets/sets the main post body.
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Gets/sets the number of available comments.
		/// </summary>
		public int CommentCount { get; set; }
		#endregion

		#region Navigation properties
		/// <summary>
		/// Gets/sets the currently selected media attachments.
		/// </summary>
		public IList<Media> Attachments { get; set; }

		/// <summary>
		/// Gets/sets the currently selected categories.
		/// </summary>
		public IList<Category> Categories { get; set; }

		/// <summary>
		/// Gets/sets the currently available comments.
		/// </summary>
		public IList<Comment> Comments { get; set; }
		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Post() {
			Attachments = new List<Media>();
			Categories = new List<Category>();
			Comments = new List<Comment>();
		}

		#region Events
		/// <summary>
		/// Called when the model is materialized by the DbContext.
		/// </summary>
		/// <param name="db">The current db context</param>
		public override void OnLoad() {
			if (Hooks.Models.Post.OnLoad != null)
				Hooks.Models.Post.OnLoad(this);
		}

		/// <summary>
		/// Called before the model is saved by the DbContext.
		/// </summary>
		/// <param name="db">The current db context</param>
		public override void OnSave() {
			// ensure to call the base class OnSave which will validate the model
			base.OnSave();

			if (Hooks.Models.Post.OnSave != null)
				Hooks.Models.Post.OnSave(this);

			// Remove from model cache
			App.ModelCache.Remove<Models.Post>(this.Id);
		}

		/// <summary>
		/// Called before the model is deleted by the DbContext.
		/// </summary>
		/// <param name="db">The current db context</param>
		public override void OnDelete() {
			if (Hooks.Models.Post.OnDelete != null)
				Hooks.Models.Post.OnDelete(this);

			// Remove from model cache
			App.ModelCache.Remove<Models.Post>(this.Id);
		}

		#endregion

		/// <summary>
		/// Method to validate model
		/// </summary>
		/// <returns>Returns the result of validation</returns>
		protected override FluentValidation.Results.ValidationResult Validate()
		{
			var validator = new PostValidator();
			return validator.Validate(this);
		}

		#region Validator
		private class PostValidator : AbstractValidator<Post>
		{
			public PostValidator()
			{
				RuleFor(m => m.Title).NotEmpty();
				RuleFor(m => m.Title).Length(0, 128);
				RuleFor(m => m.Keywords).Length(0, 128);
				RuleFor(m => m.Description).Length(0, 255);
				RuleFor(m => m.Route).Length(0, 255);
				RuleFor(m => m.View).Length(0, 255);
				RuleFor(m => m.Excerpt).Length(0, 512);

				// check unique TypeId+Slug
				RuleFor(m => m.Slug).Must((m, slug) => { return IsTypeIdPlusSlugUnique(m, slug); }).WithMessage("TypeId and Slug combination should be unique");
			}

			/// <summary>
			/// Function to validate if TypeId+Slug is unique
			/// </summary>
			/// <param name="slug"></param>
			/// <param name="api"></param>
			/// <returns></returns>
			private bool IsTypeIdPlusSlugUnique(Post p, string slug)
			{
				using (var api = new Api())
				{
					var recordCount = api.Posts.Get(where: m => m.Slug == slug && m.TypeId == p.TypeId && m.Id != p.Id).Count();
					return recordCount == 0;	
				}
			}

		}
		#endregion
	}
}
