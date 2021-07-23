import { Component, Input, OnInit } from '@angular/core';
import { AccountService } from 'src/app/account/account.service';
import { IComment } from 'src/app/shared/models/comment';
import { ShopService } from '../shop.service';

@Component({
  selector: 'app-product-comment',
  templateUrl: './product-comment.component.html',
  styleUrls: ['./product-comment.component.scss']
})

export class ProductCommentComponent implements OnInit {
  @Input() productId: string;
  constructor(
    public readonly accountService: AccountService,
    private readonly shopService: ShopService) {
  }

  data: IComment[] = [];
  submitting = false;
  user = {
    author: '',
    avatar: 'https://zos.alipayobjects.com/rmsportal/ODTLcjxAfvqbxHnVXCYX.png'
  };
  inputValue = '';

  formatDate(date: string): string {
    const transter = new Date(date);
    console.log(transter);
    return transter.toLocaleString();

  }
  handleSubmit(): void {
    this.submitting = true;
    const content = this.inputValue;
    this.inputValue = '';

    this.shopService.postComments(content, this.productId).subscribe((comment: any) => {
      this.submitting = false;

      return this.data = [... this.data,
      {
        author: comment.userName,
        content: comment.content,
        datetime: comment.commentAt,
        displayTime: this.formatDate(comment.commentAt),
        avatar: 'https://zos.alipayobjects.com/rmsportal/ODTLcjxAfvqbxHnVXCYX.png'
      }];
    }, error => {
      this.submitting = false;
    });
  }
  ngOnInit(): void {
    this.accountService.currentUser$.subscribe(user =>
      this.user.author = user.displayName);
    this.shopService.getComments(this.productId).subscribe((comments: any) => {
      comments.forEach((comment => {
        this.data = [... this.data,
        {
          author: comment.userName,
          content: comment.content,
          datetime: comment.commentAt,
          displayTime: this.formatDate(comment.commentAt),
          avatar: 'https://zos.alipayobjects.com/rmsportal/ODTLcjxAfvqbxHnVXCYX.png'
        }];
      }));
    });
  }
}


