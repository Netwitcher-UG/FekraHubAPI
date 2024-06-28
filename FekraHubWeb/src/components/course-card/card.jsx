// MUI Imports
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Typography from '@mui/material/Typography'

import classnames from 'classnames'

import OptionMenu from '@core/components/option-menu'

// Third-party Imports

// Component Imports
import CustomAvatar from '@core/components/mui/Avatar'

const HorizontalWithSubtitle = props => {
  // Props
  const { firstName, lastName, name, startDate, endDate, price, moreOptions, avatarIcon, avatarColor } = props
  const start = startDate.toString().split('T')[0]
  const end = endDate.toString().split('T')[0]

  return (
    <Card className='bs-full mt-4'>
      <CardContent>
      
        <div className='flex flex-col gap-1'>
          <Typography variant='h5' className='text-success font-bold'>
            {firstName} {lastName}
          </Typography>
          <div className='flex gap-2'>
            <Typography variant='body2' className='font-bold'>
              Course Name:
            </Typography>
            <Typography variant='body2' className='font-bold'>
              {name}
            </Typography>
          </div>
          <div className='flex gap-1'>
            <Typography variant='body2' className='font-bold'>
              StartDate:
            </Typography>
            <Typography variant='body2' className='w-100'>
              {start}
            </Typography>
          </div>
          <div className='flex gap-1'>
            <Typography variant='body2' className='font-bold'>
              EndDate:
            </Typography>
            <Typography variant='body2'>{end}</Typography>
          </div>
          <div className='flex gap-2'>
            <Typography variant='body2' className='font-bold'>
              Price Course:
            </Typography>
            <Typography variant='body2'> {price}</Typography>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

export default HorizontalWithSubtitle
