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
  const { firstName, lastName, avatarIcon, avatarColor, course, sDate, eDate, pCourse, moreOptions } = props

  return (
    <Card className='bs-full mt-4'>
      <CardContent>
        <div className='flex justify-between items-center is-full mbe-5'>
          <CustomAvatar color={avatarColor} skin='light' variant='rounded' size={42}>
            <i className={classnames(avatarIcon, 'text-[26px]')} />
          </CustomAvatar>
          <OptionMenu
            {...(moreOptions
              ? moreOptions
              : {
                  options: ['Reports', 'Update'],
                  iconButtonProps: { className: 'text-textPrimary' }
                })}
          />
        </div>
        <div className='flex flex-col gap-1'>
          <Typography variant='h5' className='text-success font-bold'>
            {firstName} {lastName}
          </Typography>
          <div className='flex gap-2'>
            <Typography variant='body2' className='font-bold'>
              Course Name:
            </Typography>
            <Typography variant='body2'> {course}</Typography>
          </div>
          <div className='flex gap-2'>
            <Typography variant='body2' className='font-bold'>
              StartDate :
            </Typography>
            <Typography variant='body2'> {sDate}</Typography>
          </div>
          <div className='flex gap-2'>
            <Typography variant='body2' className='font-bold'>
              EndDate:
            </Typography>
            <Typography variant='body2'> {eDate}</Typography>
          </div>
          <div className='flex gap-2'>
            <Typography variant='body2' className='font-bold'>
              Price Course:
            </Typography>
            <Typography variant='body2'> {pCourse}</Typography>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

export default HorizontalWithSubtitle
