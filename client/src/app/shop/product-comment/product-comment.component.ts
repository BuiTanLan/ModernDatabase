import { Component, OnInit } from '@angular/core';
import { AccountService } from 'src/app/account/account.service';
import { IComment } from 'src/app/shared/models/comment';

@Component({
  selector: 'app-product-comment',
  templateUrl: './product-comment.component.html',
  styleUrls: ['./product-comment.component.scss']
})

export class ProductCommentComponent implements OnInit {

  constructor(private readonly accountService: AccountService){
  }

  data: IComment[] = [];
  submitting = false;
  user = {
    author: '',
    avatar: 'https://zos.alipayobjects.com/rmsportal/ODTLcjxAfvqbxHnVXCYX.png'
  };
  inputValue = '';

  handleSubmit(): void {
    this.submitting = true;
    const content = this.inputValue;
    this.inputValue = '';
    setTimeout(() => {
      this.submitting = false;
      this.data = [
        ...this.data,
        {
          ...this.user,
          content,
          datetime: new Date(),
          displayTime: Date.now().toString()
        }
      ].map(e => {
        return {
          ...e,
          displayTime: Date()
        };
      });
    }, 800);
  }
  ngOnInit(): void {
    this.accountService.currentUser$.subscribe(user =>
      this.user.author = user.displayName);
  }
}



